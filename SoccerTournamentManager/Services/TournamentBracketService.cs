using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;

namespace SoccerTournamentManager.Services
{
    public sealed class TournamentBracketService
    {
        private readonly ApplicationDbContext _dbContext;

        public TournamentBracketService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task FinalizeDrawAsync(
            int tournamentId,
            string? orderedTeamIds,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var tournament = await _dbContext.Tournaments
                .SingleOrDefaultAsync(item => item.TournamentId == tournamentId, cancellationToken)
                ?? throw new KeyNotFoundException("Tournament not found.");
            var teamIds = await _dbContext.TournamentTeams
                .Where(item => item.TournamentId == tournamentId)
                .Select(item => item.TeamId)
                .Distinct()
                .ToListAsync(cancellationToken);
            if (teamIds.Count < 2)
            {
                throw new InvalidOperationException("Add at least two teams to the tournament before creating a draw.");
            }

            var existingDraw = await _dbContext.TournamentDraws
                .SingleOrDefaultAsync(draw => draw.TournamentId == tournamentId, cancellationToken);
            var existingMatches = await _dbContext.Matches
                .AnyAsync(match => match.TournamentId == tournamentId, cancellationToken);
            if (existingDraw != null || existingMatches)
            {
                throw new InvalidOperationException("A draw or fixture has already been finalized. It cannot be changed or regenerated.");
            }

            var order = ParseDrawOrder(orderedTeamIds, teamIds);
            if (string.IsNullOrWhiteSpace(orderedTeamIds))
            {
                Shuffle(order);
            }

            if (existingDraw == null)
            {
                existingDraw = new TournamentDraw
                {
                    TournamentId = tournamentId
                };
                _dbContext.TournamentDraws.Add(existingDraw);
            }

            existingDraw.TeamOrderJson = JsonSerializer.Serialize(order);
            existingDraw.FinalizedAtUtc = DateTime.UtcNow;
            existingDraw.FinalizedByUserId = userId;

            var normalizedFormat = tournament.Format?.Trim().ToLowerInvariant();
            if (normalizedFormat == "league" || normalizedFormat == "round robin")
            {
                AddRoundRobinFixtures(tournamentId, order, "League");
            }
            else if (normalizedFormat == "group stage")
            {
                AddGroupFixtures(tournamentId, order);
            }
            else
            {
                AddOpeningKnockoutRound(tournamentId, order);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AdvanceKnockoutAsync(int tournamentId, CancellationToken cancellationToken = default)
        {
            var draw = await _dbContext.TournamentDraws
                .SingleOrDefaultAsync(item => item.TournamentId == tournamentId, cancellationToken);
            if (draw == null)
                return;

            var teamOrder = JsonSerializer.Deserialize<List<int>>(draw.TeamOrderJson) ?? [];
            var bracketSize = NextPowerOfTwo(teamOrder.Count);
            var roundCount = (int)Math.Log2(bracketSize);
            var matches = await _dbContext.Matches
                .Where(match => match.TournamentId == tournamentId && match.BracketRound.HasValue)
                .ToListAsync(cancellationToken);
            var latestRound = matches.Count == 0 ? 0 : matches.Max(match => match.BracketRound!.Value);

            if (latestRound == 0)
            {
                var groupTournament = await _dbContext.Tournaments
                    .SingleOrDefaultAsync(item => item.TournamentId == tournamentId, cancellationToken);
                if (!string.Equals(groupTournament?.Format, "Group Stage", StringComparison.OrdinalIgnoreCase))
                    return;

                var groupMatches = await _dbContext.Matches
                    .Where(match => match.TournamentId == tournamentId && match.Stage != null
                        && match.Stage.StartsWith("Group "))
                    .ToListAsync(cancellationToken);
                if (groupMatches.Count == 0 || groupMatches.Any(match =>
                        !string.Equals(match.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                        || !match.HomeScore.HasValue || !match.AwayScore.HasValue))
                    return;

                var groups = await _dbContext.Groups
                    .Where(group => group.TournamentId == tournamentId)
                    .ToListAsync(cancellationToken);
                var groupTeams = await (from groupTeam in _dbContext.GroupTeams
                                        join groupItem in _dbContext.Groups on groupTeam.GroupId equals groupItem.GroupId
                                        join team in _dbContext.Teams on groupTeam.TeamId equals team.TeamId
                                        where groupItem.TournamentId == tournamentId
                                        select new
                                        {
                                            groupItem.GroupName,
                                            team.TeamId,
                                            team.TeamName
                                        }).ToListAsync(cancellationToken);
                var qualifiers = new List<int>();
                foreach (var group in groups.OrderBy(item => item.GroupName))
                {
                    var members = groupTeams.Where(item => item.GroupName == group.GroupName).ToList();
                    var groupTeamIds = members.Select(item => item.TeamId).ToHashSet();
                    var groupStandings = BuildStandings(
                        members.Select(item => new BracketTeamViewModel
                        {
                            TeamId = item.TeamId,
                            Name = item.TeamName
                        }),
                        groupMatches.Where(match => match.Stage == group.GroupName
                            && groupTeamIds.Contains(match.HomeTeamId)
                            && groupTeamIds.Contains(match.AwayTeamId)),
                        groupTournament.PointsWin,
                        groupTournament.PointsDraw);
                    qualifiers.AddRange(groupStandings.Take(2).Select(item => item.TeamId));
                }

                if (qualifiers.Count < 2)
                    return;

                draw.KnockoutTeamOrderJson = JsonSerializer.Serialize(qualifiers);
                AddOpeningKnockoutRound(tournamentId, qualifiers);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            var winners = new List<int>();
            if (latestRound == 1)
            {
                var seedPositions = GetSeedPositions(bracketSize);
                for (var position = 0; position < bracketSize / 2; position++)
                {
                    var homeSeedIndex = seedPositions[position * 2] - 1;
                    var awaySeedIndex = seedPositions[position * 2 + 1] - 1;
                    var homeTeamId = homeSeedIndex < teamOrder.Count ? teamOrder[homeSeedIndex] : (int?)null;
                    var awayTeamId = awaySeedIndex < teamOrder.Count ? teamOrder[awaySeedIndex] : (int?)null;

                    if (homeTeamId.HasValue && awayTeamId.HasValue)
                    {
                        var match = matches.SingleOrDefault(item => item.BracketRound == 1 && item.BracketPosition == position);
                        if (match == null || !string.Equals(match.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                            return;
                        var winner = ResolveWinner(match);
                        if (!winner.HasValue)
                            return;
                        winners.Add(winner.Value);
                    }
                    else if (homeTeamId.HasValue || awayTeamId.HasValue)
                    {
                        winners.Add(homeTeamId ?? awayTeamId!.Value);
                    }
                }
            }
            else
            {
                var previousRound = matches
                    .Where(match => match.BracketRound == latestRound)
                    .OrderBy(match => match.BracketPosition)
                    .ToList();
                var expectedMatches = bracketSize / (int)Math.Pow(2, latestRound);
                if (previousRound.Count != expectedMatches
                    || previousRound.Any(match => !string.Equals(match.Status, "Completed", StringComparison.OrdinalIgnoreCase)))
                    return;

                foreach (var match in previousRound)
                {
                    var winner = ResolveWinner(match);
                    if (!winner.HasValue)
                        return;
                    winners.Add(winner.Value);
                }
            }

            var nextRound = latestRound + 1;
            if (nextRound > roundCount || winners.Count < 2)
                return;

            var tournamentDate = await _dbContext.Tournaments
                .Where(item => item.TournamentId == tournamentId)
                .Select(item => item.StartDate)
                .SingleOrDefaultAsync(cancellationToken) ?? DateTime.Today;

            for (var position = 0; position < winners.Count / 2; position++)
            {
                _dbContext.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    HomeTeamId = winners[position * 2],
                    AwayTeamId = winners[position * 2 + 1],
                    MatchDate = tournamentDate.AddDays(nextRound),
                    Stage = RoundName(bracketSize, nextRound),
                    Status = "Upcoming",
                    BracketRound = nextRound,
                    BracketPosition = position
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<TournamentBracketViewModel?> GetBracketAsync(
            int tournamentId,
            CancellationToken cancellationToken = default)
        {
            var tournament = await _dbContext.Tournaments
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.TournamentId == tournamentId, cancellationToken);
            if (tournament == null)
                return null;

            var teams = await (from member in _dbContext.TournamentTeams.AsNoTracking()
                               join team in _dbContext.Teams.AsNoTracking() on member.TeamId equals team.TeamId
                               where member.TournamentId == tournamentId
                               select new BracketTeamViewModel { TeamId = team.TeamId, Name = team.TeamName })
                .Distinct()
                .ToListAsync(cancellationToken);
            var draw = await _dbContext.TournamentDraws
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.TournamentId == tournamentId, cancellationToken);
            var drawOrder = draw == null
                ? teams.Select(team => team.TeamId).ToList()
                : JsonSerializer.Deserialize<List<int>>(draw.TeamOrderJson) ?? [];
            var teamById = teams.ToDictionary(team => team.TeamId);
            var orderedTeams = drawOrder.Where(teamById.ContainsKey).Select(teamId => teamById[teamId]).ToList();
            var knockoutOrder = draw?.KnockoutTeamOrderJson == null
                ? drawOrder
                : JsonSerializer.Deserialize<List<int>>(draw.KnockoutTeamOrderJson) ?? [];
            var knockoutTeams = knockoutOrder.Where(teamById.ContainsKey)
                .Select(teamId => teamById[teamId]).ToList();
            var matches = await _dbContext.Matches
                .AsNoTracking()
                .Include(match => match.HomeTeam)
                .Include(match => match.AwayTeam)
                .Where(match => match.TournamentId == tournamentId)
                .OrderBy(match => match.MatchDate)
                .ToListAsync(cancellationToken);

            var groupRows = await _dbContext.Groups
                .AsNoTracking()
                .Where(group => group.TournamentId == tournamentId)
                .OrderBy(group => group.GroupName)
                .ToListAsync(cancellationToken);
            var groupTeamRows = await (from groupTeam in _dbContext.GroupTeams.AsNoTracking()
                                       join groupItem in _dbContext.Groups.AsNoTracking()
                                           on groupTeam.GroupId equals groupItem.GroupId
                                       join team in _dbContext.Teams.AsNoTracking()
                                           on groupTeam.TeamId equals team.TeamId
                                       where groupItem.TournamentId == tournamentId
                                       select new
                                       {
                                           groupItem.GroupName,
                                           team.TeamId,
                                           team.TeamName
                                       }).ToListAsync(cancellationToken);
            var groups = groupRows.Select(group =>
            {
                var groupMembers = groupTeamRows
                    .Where(item => item.GroupName == group.GroupName)
                    .OrderBy(item => item.TeamName)
                    .ToList();
                var groupMemberIds = groupMembers.Select(item => item.TeamId).ToHashSet();
                var groupMatches = matches.Where(match => match.Stage == group.GroupName
                    && groupMemberIds.Contains(match.HomeTeamId)
                    && groupMemberIds.Contains(match.AwayTeamId));
                return new BracketGroupViewModel
                {
                    Name = group.GroupName,
                    Teams = groupMembers.Select(item => new BracketTeamViewModel
                    {
                        TeamId = item.TeamId,
                        Name = item.TeamName
                    })
                    .ToList(),
                    Standings = BuildStandings(
                        groupMembers.Select(item => new BracketTeamViewModel
                        {
                            TeamId = item.TeamId,
                            Name = item.TeamName
                        }),
                        groupMatches,
                        tournament.PointsWin,
                        tournament.PointsDraw)
                };
            }).ToList();

            var standings = BuildStandings(teams, matches, tournament.PointsWin, tournament.PointsDraw);
            var isKnockout = !string.Equals(tournament.Format, "League", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(tournament.Format, "Round Robin", StringComparison.OrdinalIgnoreCase)
                && (!string.Equals(tournament.Format, "Group Stage", StringComparison.OrdinalIgnoreCase)
                    || draw?.KnockoutTeamOrderJson != null);
            var rounds = isKnockout ? BuildRounds(knockoutTeams, matches) : [];

            return new TournamentBracketViewModel
            {
                TournamentId = tournamentId,
                TournamentName = tournament.Name,
                Format = tournament.Format ?? "Knockout",
                HasDraw = draw != null,
                DrawOrder = drawOrder,
                Teams = orderedTeams,
                Groups = groups,
                Rounds = rounds,
                Standings = standings,
                MatchesGenerated = matches.Count
            };
        }

        private void AddOpeningKnockoutRound(int tournamentId, IReadOnlyList<int> order)
        {
            var bracketSize = NextPowerOfTwo(order.Count);
            var seedPositions = GetSeedPositions(bracketSize);
            for (var position = 0; position < bracketSize / 2; position++)
            {
                var homeIndex = seedPositions[position * 2] - 1;
                var awayIndex = seedPositions[position * 2 + 1] - 1;
                if (homeIndex >= order.Count || awayIndex >= order.Count)
                    continue;

                _dbContext.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    HomeTeamId = order[homeIndex],
                    AwayTeamId = order[awayIndex],
                    MatchDate = DateTime.Today.AddDays(1),
                    Stage = RoundName(bracketSize, 1),
                    Status = "Upcoming",
                    BracketRound = 1,
                    BracketPosition = position
                });
            }
        }

        private void AddGroupFixtures(int tournamentId, IReadOnlyList<int> order)
        {
            const int maximumTeamsPerGroup = 4;
            var groupCount = (int)Math.Ceiling(order.Count / (double)maximumTeamsPerGroup);
            var groups = Enumerable.Range(0, groupCount)
                .Select(index => new Group
                {
                    GroupName = $"Group {(char)('A' + index)}",
                    TournamentId = tournamentId
                })
                .ToList();
            _dbContext.Groups.AddRange(groups);
            for (var index = 0; index < order.Count; index++)
            {
                var group = groups[index % groupCount];
                _dbContext.GroupTeams.Add(new GroupTeam { Group = group, TeamId = order[index] });
            }

            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var groupTeamIds = order
                    .Where((_, teamIndex) => teamIndex % groupCount == groupIndex)
                    .ToList();
                AddRoundRobinFixtures(tournamentId, groupTeamIds, groups[groupIndex].GroupName);
            }
        }

        private void AddRoundRobinFixtures(int tournamentId, IReadOnlyList<int> teamIds, string stage)
        {
            var day = 1;
            for (var home = 0; home < teamIds.Count; home++)
            {
                for (var away = home + 1; away < teamIds.Count; away++)
                {
                    _dbContext.Matches.Add(new Match
                    {
                        TournamentId = tournamentId,
                        HomeTeamId = teamIds[home],
                        AwayTeamId = teamIds[away],
                        MatchDate = DateTime.Today.AddDays(day++),
                        Stage = stage,
                        Status = "Upcoming"
                    });
                }
            }
        }

        private static List<int> ParseDrawOrder(string? submitted, IReadOnlyCollection<int> eligibleTeamIds)
        {
            if (string.IsNullOrWhiteSpace(submitted))
                return eligibleTeamIds.ToList();

            List<int>? order;
            try
            {
                order = JsonSerializer.Deserialize<List<int>>(submitted);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("The submitted seed order is invalid. Refresh the page and try again.");
            }

            if (order == null
                || order.Count != eligibleTeamIds.Count
                || order.Distinct().Count() != order.Count
                || !order.ToHashSet().SetEquals(eligibleTeamIds))
            {
                throw new InvalidOperationException("Choose each tournament team exactly once in the seed order.");
            }

            return order;
        }

        private static IReadOnlyList<BracketRoundViewModel> BuildRounds(
            IReadOnlyList<BracketTeamViewModel> orderedTeams,
            IReadOnlyList<Match> matches)
        {
            if (orderedTeams.Count < 2)
                return [];

            var bracketSize = NextPowerOfTwo(orderedTeams.Count);
            var roundCount = (int)Math.Log2(bracketSize);
            var seedPositions = GetSeedPositions(bracketSize);
            var rounds = new List<BracketRoundViewModel>();
            var priorRoundWinners = new List<string>();
            for (var roundNumber = 1; roundNumber <= roundCount; roundNumber++)
            {
                var bracketMatches = new List<BracketMatchViewModel>();
                var roundWinners = new List<string>();
                var matchCount = bracketSize / (int)Math.Pow(2, roundNumber);
                for (var position = 0; position < matchCount; position++)
                {
                    string home;
                    string away;
                    if (roundNumber == 1)
                    {
                        var homeIndex = seedPositions[position * 2] - 1;
                        var awayIndex = seedPositions[position * 2 + 1] - 1;
                        home = homeIndex < orderedTeams.Count ? orderedTeams[homeIndex].Name : "BYE";
                        away = awayIndex < orderedTeams.Count ? orderedTeams[awayIndex].Name : "BYE";
                    }
                    else
                    {
                        home = priorRoundWinners.ElementAtOrDefault(position * 2) ?? "TBD";
                        away = priorRoundWinners.ElementAtOrDefault(position * 2 + 1) ?? "TBD";
                    }

                    var match = matches.FirstOrDefault(item =>
                        item.BracketRound == roundNumber && item.BracketPosition == position);
                    if (match != null)
                    {
                        home = match.HomeTeam?.TeamName ?? home;
                        away = match.AwayTeam?.TeamName ?? away;
                    }

                    var winner = match == null ? null : ResolveWinner(match);
                    bracketMatches.Add(new BracketMatchViewModel
                    {
                        MatchId = match?.MatchId,
                        HomeTeam = home,
                        AwayTeam = away,
                        HomeScore = match?.HomeScore,
                        AwayScore = match?.AwayScore,
                        Status = match?.Status ?? (home == "BYE" || away == "BYE" ? "Bye" : "TBD")
                    });

                    if (roundNumber < roundCount)
                    {
                        var byeWinner = home == "BYE" ? away : away == "BYE" ? home : null;
                        roundWinners.Add(winner.HasValue
                            ? match!.HomeTeamId == winner ? home : away
                            : byeWinner ?? "TBD");
                    }
                }

                rounds.Add(new BracketRoundViewModel
                {
                    Name = RoundName(bracketSize, roundNumber),
                    Matches = bracketMatches
                });
                priorRoundWinners = roundWinners;
            }

            return rounds;
        }

        private static IReadOnlyList<LeagueStandingViewModel> BuildStandings(
            IEnumerable<BracketTeamViewModel> teams,
            IEnumerable<Match> matches,
            int pointsWin,
            int pointsDraw)
        {
            var standings = teams.ToDictionary(
                team => team.TeamId,
                team => new MutableStanding(team.TeamId, team.Name));

            foreach (var match in matches.Where(match =>
                         string.Equals(match.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                         && match.HomeScore.HasValue
                         && match.AwayScore.HasValue
                         && standings.ContainsKey(match.HomeTeamId)
                         && standings.ContainsKey(match.AwayTeamId)))
            {
                var home = standings[match.HomeTeamId];
                var away = standings[match.AwayTeamId];
                home.Played++;
                away.Played++;
                home.GoalsFor += match.HomeScore!.Value;
                home.GoalsAgainst += match.AwayScore!.Value;
                away.GoalsFor += match.AwayScore.Value;
                away.GoalsAgainst += match.HomeScore.Value;

                if (match.HomeScore > match.AwayScore)
                {
                    home.Won++;
                    home.Points += pointsWin;
                    away.Lost++;
                }
                else if (match.HomeScore < match.AwayScore)
                {
                    away.Won++;
                    away.Points += pointsWin;
                    home.Lost++;
                }
                else
                {
                    home.Drawn++;
                    away.Drawn++;
                    home.Points += pointsDraw;
                    away.Points += pointsDraw;
                }
            }

            return standings.Values
                .OrderByDescending(team => team.Points)
                .ThenByDescending(team => team.GoalsFor - team.GoalsAgainst)
                .ThenByDescending(team => team.GoalsFor)
                .ThenBy(team => team.TeamName)
                .Select((team, index) => new LeagueStandingViewModel
                {
                    Position = index + 1,
                    TeamId = team.TeamId,
                    TeamName = team.TeamName,
                    Played = team.Played,
                    Won = team.Won,
                    Drawn = team.Drawn,
                    Lost = team.Lost,
                    GoalsFor = team.GoalsFor,
                    GoalsAgainst = team.GoalsAgainst,
                    Points = team.Points
                })
                .ToList();
        }

        private static int? ResolveWinner(Match match)
        {
            if (match.WinnerTeamId == match.HomeTeamId || match.WinnerTeamId == match.AwayTeamId)
                return match.WinnerTeamId;
            if (match.HomeScore > match.AwayScore)
                return match.HomeTeamId;
            if (match.AwayScore > match.HomeScore)
                return match.AwayTeamId;
            return null;
        }

        private static int NextPowerOfTwo(int count)
        {
            var size = 1;
            while (size < count)
                size *= 2;
            return size;
        }

        private static List<int> GetSeedPositions(int size)
        {
            var positions = new List<int> { 1, 2 };
            while (positions.Count < size)
            {
                var nextSize = positions.Count * 2;
                positions = positions
                    .SelectMany(seed => new[] { seed, nextSize + 1 - seed })
                    .ToList();
            }
            return positions;
        }

        private static string RoundName(int bracketSize, int roundNumber)
        {
            var teamsInRound = bracketSize / (int)Math.Pow(2, roundNumber - 1);
            return teamsInRound switch
            {
                2 => "Final",
                4 => "Semi-Final",
                8 => "Quarter-Final",
                _ => $"Round of {teamsInRound}"
            };
        }

        private static void Shuffle(IList<int> values)
        {
            for (var index = values.Count - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }
        }

        private sealed class MutableStanding(int teamId, string teamName)
        {
            public int TeamId { get; } = teamId;
            public string TeamName { get; } = teamName;
            public int Played { get; set; }
            public int Won { get; set; }
            public int Drawn { get; set; }
            public int Lost { get; set; }
            public int GoalsFor { get; set; }
            public int GoalsAgainst { get; set; }
            public int Points { get; set; }
        }
    }
}

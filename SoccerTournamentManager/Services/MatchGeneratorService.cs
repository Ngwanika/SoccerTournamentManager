using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using System;
using System.Linq;

namespace SoccerTournamentManager.Services
{
    public class MatchGeneratorService
    {

        private readonly IUnitOfWork _unitOfWork;

        public MatchGeneratorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void GenerateLeagueMatches(int tournamentId)
        {
            // Check if matches already exist
            var existingMatches = _unitOfWork.Match
                .GetAll()
                .Where(x => x.TournamentId == tournamentId)
                .ToList();

            if (existingMatches.Count > 0)
                return;


            // Get teams in tournament
            var tournamentTeams = _unitOfWork.TournamentTeams
                .GetAll()
                .Where(x => x.TournamentId == tournamentId)
                .ToList();

            var teamIds = tournamentTeams
                .Select(x => x.TeamId)
                .ToList();


            // Generate round robin matches
            for (int i = 0; i < teamIds.Count; i++)
            {
                for (int j = i + 1; j < teamIds.Count; j++)
                {
                    Match match = new Match()
                    {
                        TournamentId = tournamentId,
                        HomeTeamId = teamIds[i],
                        AwayTeamId = teamIds[j],
                        MatchDate = DateTime.Now.AddDays(i + j),
                        Stage = "League",
                        Status = "Scheduled",
                        HomeScore = null,
                        AwayScore = null
                    };

                    _unitOfWork.Match.Add(match);
                }
            }

            _unitOfWork.Save();
        }
    }
}

using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public ITeamRepository Team { get; private set; }
        public IPlayerRepository Player { get; private set; }
        public ITournamentRepositiry Tournament { get; private set; }
        public ITournamentTeamsRepository TournamentTeams { get; private set; }
        public IGroupTeamRepository GroupTeam { get; private set; }
        public IGroupRepository Group { get; private set; }
        public ITournamentFormatRepository TournamentFormat { get; private set; }

        public IMatchRepository Match { get; private set; }

        public ITournamentRegistrationRepository TournamentRegistration { get; private set; }

        public IGoalRepository Goal { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Team = new TeamRepository(_db);
            Player = new PlayerRepository(_db);
            Tournament = new TournamentRepository(_db);
            TournamentTeams = new TournamentTeamsRepository(_db);
            GroupTeam = new GroupTeamRepository(_db);
            Group = new GroupRepository(_db);
            TournamentFormat = new TournamentFormatRepository(_db);
            Match = new MatchRepository(_db);
            TournamentRegistration = new TournamentRegistrationRepository(_db);
            Goal = new GoalRepository(_db);


        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}

using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.DataAccess.Repository
{
    public class TournamentTeamsRepository : Repository<TournamentTeam>, ITournamentTeamsRepository
    {

        private ApplicationDbContext _db;
        public TournamentTeamsRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(TournamentTeam obj)
        {
            _db.TournamentTeams.Update(obj);
        }
    }
}


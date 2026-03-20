using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.DataAccess.Repository;
using SoccerTournament.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.DataAccess.Repository
{
    public class TournamentRepository : Repository<Tournament>, ITournamentRepositiry
    {
        private ApplicationDbContext _db;
        public TournamentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Tournament obj)
        {
            _db.Tournaments.Update(obj);
        }
    }
}




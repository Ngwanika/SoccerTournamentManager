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
    public class MatchRepository : Repository<Match>, IMatchRepository
    {
        private ApplicationDbContext _db;
        public MatchRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }       

        public void Update(Match obj)
        {
           _db.Matches.Update(obj);
        }
    }
}

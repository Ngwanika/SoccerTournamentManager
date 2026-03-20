using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.DataAccess.Repository
{
    public class TournamentFormatRepository : Repository<TournamentFormat>, ITournamentFormatRepository
    {
        private ApplicationDbContext _db;
        public TournamentFormatRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(TournamentFormat obj)
        {
            _db.TournamentFormat.Update(obj);
        }
    }
}




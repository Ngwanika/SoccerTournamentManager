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
    public class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        private ApplicationDbContext _db;
        public PlayerRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Player obj)
        {
            _db.Players.Update(obj);
        }
    }
}



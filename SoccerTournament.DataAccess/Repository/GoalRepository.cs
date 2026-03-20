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
    public class GoalRepository : Repository<Goal>, IGoalRepository
    {
        private ApplicationDbContext _db;
        public GoalRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Goal obj)
        {
            _db.Goals.Update(obj);
        }
    }
}




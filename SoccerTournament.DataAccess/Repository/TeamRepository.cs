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
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        private ApplicationDbContext _db;
        public TeamRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }       

        public void Update(Team obj)
        {
            _db.Teams.Update(obj);
        }
    }
}

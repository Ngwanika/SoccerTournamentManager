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
    public class GroupTeamRepository : Repository<GroupTeam>, IGroupTeamRepository
    {
        private ApplicationDbContext _db;
        public GroupTeamRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(GroupTeam obj)
        {
            _db.GroupTeams.Update(obj);
        }
    }
}



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
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        private ApplicationDbContext _db;
        public GroupRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Group obj)
        {
            _db.Groups.Update(obj);
        }
    }
}




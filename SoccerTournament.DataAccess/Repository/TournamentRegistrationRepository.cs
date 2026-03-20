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
    public class TournamentRegistrationRepository : Repository<TournamentRegistration>, ITournamentRegistrationRepository
    {
        private ApplicationDbContext _db;
        public TournamentRegistrationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(TournamentRegistration obj)
        {
            _db.TournamentRegistrations.Update(obj);
        }
    }
}




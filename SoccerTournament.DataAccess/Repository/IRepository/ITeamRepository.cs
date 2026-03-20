using SoccerTournament.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoccerTournament.DataAccess.Repository.IRepository
{
    public interface ITeamRepository : IRepository<Team>
    {
        void Update(Team obj);
      
    }
}

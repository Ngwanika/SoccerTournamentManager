using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoccerTournament.DataAccess.Repository.IRepository;

namespace SoccerTournament.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ITeamRepository Team {  get; }
        IPlayerRepository Player { get; }
        ITournamentRepositiry Tournament { get; }
        ITournamentTeamsRepository TournamentTeams { get; }
        IGroupTeamRepository GroupTeam { get; }
        IGroupRepository Group { get; }
        ITournamentFormatRepository TournamentFormat { get; }
        IMatchRepository Match { get; }
        ITournamentRegistrationRepository TournamentRegistration { get; }
        IGoalRepository Goal { get; }   
        void Save();
    }
}

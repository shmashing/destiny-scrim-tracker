using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IMatchTeamRepository
    {
        IEnumerable<string> WriteMatchTeams(IEnumerable<MatchTeam> matchTeams);
        IEnumerable<MatchTeam> GetTeamsForMatch(string matchId);
        IEnumerable<string> DeleteTeamsForMatch(string matchId);
    }
    
    public class MatchTeamRepository : IMatchTeamRepository
    {
        private readonly DatabaseContext _databaseContext;

        public MatchTeamRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public IEnumerable<string> WriteMatchTeams(IEnumerable<MatchTeam> matchTeams)
        {
            _databaseContext.AddRange(matchTeams);
            _databaseContext.SaveChanges();

            return matchTeams.Select(team => team.Id);
        }

        public IEnumerable<MatchTeam> GetTeamsForMatch(string matchId)
        {
            return _databaseContext.MatchTeams.Where(mt => mt.MatchId == matchId).ToList();
        }

        public IEnumerable<string> DeleteTeamsForMatch(string matchId)
        {
            var teams = _databaseContext.MatchTeams.Where(mt => mt.MatchId == matchId);
            
            _databaseContext.RemoveRange(teams);
            _databaseContext.SaveChanges();

            return teams.Select(t => t.Id);
        }
    }
}
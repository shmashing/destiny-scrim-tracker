using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IMatchTeamRepository
    {
        Task<IEnumerable<string>> WriteMatchTeams(IEnumerable<MatchTeam> matchTeams);
        Task<IEnumerable<MatchTeam>> GetTeamsForMatch(string matchId);
        Task<IEnumerable<string>> DeleteTeamsForMatch(string matchId);
    }
    
    public class MatchTeamRepository : IMatchTeamRepository
    {
        private readonly DatabaseContext _databaseContext;

        public MatchTeamRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public async Task<IEnumerable<string>> WriteMatchTeams(IEnumerable<MatchTeam> matchTeams)
        {
            await _databaseContext.AddRangeAsync(matchTeams);
            await _databaseContext.SaveChangesAsync();

            return matchTeams.Select(team => team.Id);
        }

        public async Task<IEnumerable<MatchTeam>> GetTeamsForMatch(string matchId)
        {
            return await _databaseContext.MatchTeams.Where(mt => mt.MatchId == matchId).ToListAsync();
        }

        public async Task<IEnumerable<string>> DeleteTeamsForMatch(string matchId)
        {
            var teams = _databaseContext.MatchTeams.Where(mt => mt.MatchId == matchId);
            
            _databaseContext.RemoveRange(teams);
            await _databaseContext.SaveChangesAsync();

            return teams.Select(t => t.Id);
        }
    }
}
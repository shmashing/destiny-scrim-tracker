using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IMatchRepository
    {
        Task<string> CreateMatch(Match match);
        IEnumerable<Match> GetAllMatches();
        Match GetMatch(string matchId);
        Task<string> DeleteMatch(string matchId);
    }
    
    public class MatchRepository : IMatchRepository
    {
        private readonly DatabaseContext _databaseContext;

        public MatchRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public async Task<string> CreateMatch(Match match)
        {
            await _databaseContext.AddAsync(match);
            await _databaseContext.SaveChangesAsync();

            return match.Id;
        }

        public IEnumerable<Match> GetAllMatches()
        {
            var matches = _databaseContext.Matches.OrderByDescending(match => match.TimeStamp).ToList();
            return matches;
        }

        public Match GetMatch(string matchId)
        {
            return _databaseContext.Matches.FirstOrDefault(match => match.Id == matchId);
        }

        public async Task<string> DeleteMatch(string matchId)
        {
            var match = _databaseContext.Matches.FirstOrDefault(match => match.Id == matchId);

            _databaseContext.Remove(match);
            await _databaseContext.SaveChangesAsync();

            return matchId;
        }
    }
}
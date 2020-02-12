using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IMatchRepository
    {
        string CreateMatch(Match match);
        IEnumerable<Match> GetAllMatches();
        Match GetMatch(string matchId);
        string DeleteMatch(string matchId);
    }
    
    public class MatchRepository : IMatchRepository
    {
        private readonly DatabaseContext _databaseContext;

        public MatchRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public string CreateMatch(Match match)
        {
            _databaseContext.Add(match);
            _databaseContext.SaveChanges();

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

        public string DeleteMatch(string matchId)
        {
            var match = _databaseContext.Matches.FirstOrDefault(match => match.Id == matchId);

            if (match == null)
            {
                return "Record not found";
            }

            _databaseContext.Remove(match);
            _databaseContext.SaveChanges();

            return matchId;
        }
    }
}
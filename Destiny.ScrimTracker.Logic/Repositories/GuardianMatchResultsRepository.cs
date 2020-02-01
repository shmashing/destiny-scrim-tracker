using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IGuardianMatchResultsRepository
    {
        IEnumerable<string> SaveGuardianResults(IEnumerable<GuardianMatchResult> matchResults);
        IEnumerable<GuardianMatchResult> GetGuardianMatchResults(string matchId, string teamId);
        IEnumerable<GuardianMatchResult> GetMatchResultsForGuardian(string guardianId);
        IEnumerable<string> DeleteGuardianResults(string matchId);
        IEnumerable<string> DeleteAllResultsForGuardian(string guardianId);
    }
    
    public class GuardianMatchResultsRepository : IGuardianMatchResultsRepository
    {
        private readonly DatabaseContext _databaseContext;

        public GuardianMatchResultsRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }


        public IEnumerable<string> SaveGuardianResults(IEnumerable<GuardianMatchResult> matchResults)
        {
            _databaseContext.AddRange(matchResults);
            _databaseContext.SaveChanges();

            return matchResults.Select(results => results.Id);
        }

        public IEnumerable<GuardianMatchResult> GetGuardianMatchResults(string matchId, string teamId)
        {
            return _databaseContext.GuardianMatchResults.Where(res =>
                res.MatchId == matchId && res.MatchTeamId == teamId).ToList();
        }

        public IEnumerable<GuardianMatchResult> GetMatchResultsForGuardian(string guardianId)
        {
            var matchResults = _databaseContext.GuardianMatchResults.Where(res => res.GuardianId == guardianId);

            if (matchResults == null || !matchResults.Any())
            {
                return new GuardianMatchResult[] {};
            }

            return matchResults;
        }

        public IEnumerable<string> DeleteGuardianResults(string matchId)
        {
            var results = _databaseContext.GuardianMatchResults.Where(gmr => gmr.MatchId == matchId);
            
            _databaseContext.RemoveRange(results);
            _databaseContext.SaveChanges();

            return results.Select(gmr => gmr.Id);
        }

        public IEnumerable<string> DeleteAllResultsForGuardian(string guardianId)
        {
            var results = _databaseContext.GuardianMatchResults.Where(gmr => gmr.GuardianId == guardianId);

            if (results == null || !results.Any())
            {
                return null;
            }
            
            _databaseContext.RemoveRange(results);
            _databaseContext.SaveChanges();

            return results.Select(gmr => gmr.Id);
        }
    }
}
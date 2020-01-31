using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IGuardianEloRepository
    {
        IEnumerable<GuardianElo> GetGuardianElos(string guardianId);
        GuardianElo GetGuardianElo(string guardianId);
        void UpdateGuardianElo(GuardianElo guardianElo);
        int DeleteGuardianElos(string guardianId);
        IEnumerable<string> DeleteEloResultForMatch(string matchId);
        GuardianElo CalculateGuardianElo(Guardian guardian, bool isWinner, double efficiencyModifier, double expectedOutcome,
            int actualOutcome, string matchId);
    }
    
    public class GuardianEloRepository : IGuardianEloRepository
    {
        private readonly DatabaseContext _databaseContext;

        public GuardianEloRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        } 
        
        public IEnumerable<GuardianElo> GetGuardianElos(string guardianId)
        {
            return _databaseContext.GuardianElos.Where(elo => elo.GuardianId == guardianId && elo.MatchId != null);
            
        }

        public GuardianElo GetGuardianElo(string guardianId)
        {
            return _databaseContext.GuardianElos.Where(elo => elo.GuardianId == guardianId)
                    .OrderByDescending(elo => elo.TimeStamp).FirstOrDefault();
        }

        public void UpdateGuardianElo(GuardianElo guardianElo)
        {
            _databaseContext.Add(guardianElo);
            _databaseContext.SaveChanges();
        }

        public int DeleteGuardianElos(string guardianId)
        {
            var guardianElos = _databaseContext.GuardianElos.Where(elo => elo.GuardianId == guardianId);
            
            _databaseContext.RemoveRange(guardianElos);
            return _databaseContext.SaveChanges();
        }

        public IEnumerable<string> DeleteEloResultForMatch(string matchId)
        {
            var eloResults = _databaseContext.GuardianElos.Where(elo => elo.MatchId == matchId);

            _databaseContext.RemoveRange(eloResults);
            _databaseContext.SaveChanges();

            return eloResults.Select(elo => elo.Id);
        }

        public GuardianElo CalculateGuardianElo(Guardian guardian, bool isWinner, double efficiencyModifier, double expectedOutcome, int actualOutcome, string matchId)
        {
            var currentElo = GetGuardianElo(guardian.Id).NewElo;

            var eloModifier = (double) guardian.EloModifier;
            if (isWinner)
            {
                if (efficiencyModifier >= 2.5)
                {
                    efficiencyModifier = 2.5;
                }
                eloModifier *= efficiencyModifier;
            }
            else
            {
                if (efficiencyModifier <= 0.25)
                {
                    efficiencyModifier = 0.25;
                }
                
                eloModifier /= efficiencyModifier;
            }

            var newElo = currentElo + (eloModifier * (actualOutcome - expectedOutcome));
            
            return new GuardianElo()
            {
                GuardianId = guardian.Id,
                Id = $"{ModelIDPrefixes.GuardianElo}_{Guid.NewGuid():N}",
                MatchId = matchId,
                NewElo = newElo,
                PreviousElo = currentElo
            };
        }
    }
}
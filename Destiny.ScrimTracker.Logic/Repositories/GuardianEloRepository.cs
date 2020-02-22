using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IGuardianEloRepository
    {
        IEnumerable<GuardianElo> GetGuardianElos(string guardianId);
        Task<GuardianElo> GetGuardianElo(string guardianId);
        Task UpdateGuardianElo(GuardianElo guardianElo);
        Task<int> DeleteGuardianElos(string guardianId);
        Task<IEnumerable<string>> DeleteEloResultForMatch(string matchId);
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
            var elos = _databaseContext.GuardianElos.Where(elo => elo.GuardianId == guardianId && elo.MatchId != null);

            if (elos == null || !elos.Any())
            {
                return new GuardianElo[] {};
            }

            return elos;
        }

        public async Task<GuardianElo> GetGuardianElo(string guardianId)
        {
            return await _databaseContext.GuardianElos.Where(elo => elo.GuardianId == guardianId)
                    .OrderByDescending(elo => elo.TimeStamp).FirstOrDefaultAsync();
        }

        public async Task UpdateGuardianElo(GuardianElo guardianElo)
        {
            await _databaseContext.AddAsync(guardianElo);
            await _databaseContext.SaveChangesAsync();
        }

        public async Task<int> DeleteGuardianElos(string guardianId)
        {
            var guardianElos = _databaseContext.GuardianElos.Where(elo => elo.GuardianId == guardianId);
            
            _databaseContext.RemoveRange(guardianElos);
            return await _databaseContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> DeleteEloResultForMatch(string matchId)
        {
            var eloResults = _databaseContext.GuardianElos.Where(elo => elo.MatchId == matchId);

            _databaseContext.RemoveRange(eloResults);
            await _databaseContext.SaveChangesAsync();

            return eloResults.Select(elo => elo.Id);
        }

        public GuardianElo CalculateGuardianElo(Guardian guardian, bool isWinner, double efficiencyModifier, double expectedOutcome, int actualOutcome, string matchId)
        {
            var currentElo = GetGuardianElo(guardian.Id).Result.NewElo;

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
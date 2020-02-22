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
    public interface IGuardianEfficiencyRepository
    {
        Task<GuardianEfficiency> GetGuardianEfficiency(string guardianId);
        IEnumerable<double> GetGuardianEfficiencies(string guardianId);
        IEnumerable<GuardianEfficiency> GetGuardianAverageEfficiencies(string guardianId);
        Task UpdateGuardianEfficiency(GuardianEfficiency guardianEfficiency);
        Task UpdateGuardianMatchEfficiency(IEnumerable<GuardianMatchResult> guardianMatchResults);
        Task<int> DeleteGuardianEfficiencies(string guardianId);
        Task<IEnumerable<string>> DeleteEfficienciesForMatch(string matchId);
    }
    
    public class GuardianEfficiencyRepository : IGuardianEfficiencyRepository
    {
        private readonly DatabaseContext _databaseContext;

        public GuardianEfficiencyRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public async Task<GuardianEfficiency> GetGuardianEfficiency(string guardianId)
        {
            var guardianEfficiency = await _databaseContext.GuardianEfficiencies.Where(eff => eff.GuardianId == guardianId)
                .OrderByDescending(eff => eff.TimeStamp).FirstOrDefaultAsync();

            return guardianEfficiency;
        }

        public IEnumerable<double> GetGuardianEfficiencies(string guardianId)
        {
            return _databaseContext.GuardianMatchResults.Where(eff => eff.GuardianId == guardianId && eff.MatchId != null).Select(gmr => gmr.Efficiency);
        }

        public IEnumerable<GuardianEfficiency> GetGuardianAverageEfficiencies(string guardianId)
        {
            var efficiencies = _databaseContext.GuardianEfficiencies.Where(eff => eff.GuardianId == guardianId && eff.MatchId != null);

            if (efficiencies == null || !efficiencies.Any())
            {
                return new GuardianEfficiency[] {};
            }

            return efficiencies;
        }

        public async Task UpdateGuardianEfficiency(GuardianEfficiency guardianEfficiency)
        {
            _databaseContext.Add(guardianEfficiency);
            await _databaseContext.SaveChangesAsync();
        }

        public async Task UpdateGuardianMatchEfficiency(IEnumerable<GuardianMatchResult> guardianMatchResults)
        {
            var efficiencyUpdates = new List<GuardianEfficiency>();
            foreach (var result in guardianMatchResults)
            {
                var currentEfficiencies = GetGuardianEfficiencies(result.GuardianId);
                var newEfficiency = (currentEfficiencies.Sum() + result.Efficiency) / (currentEfficiencies.Count() + 1);
                
                var newGuardianEff = new GuardianEfficiency()
                {
                    Id = $"eff_{Guid.NewGuid():N}",
                    GuardianId = result.GuardianId,
                    MatchId = result.MatchId,
                    NewEfficiency = newEfficiency,
                    PreviousEfficiency = currentEfficiencies.Any() ? currentEfficiencies.Average() : 0
                };

                efficiencyUpdates.Add(newGuardianEff);
            }

            await _databaseContext.AddRangeAsync(efficiencyUpdates);
            await _databaseContext.SaveChangesAsync();
        }

        public async Task<int> DeleteGuardianEfficiencies(string guardianId)
        {
            var guardianEfficiencies = _databaseContext.GuardianEfficiencies.Where(eff => eff.GuardianId == guardianId);

            _databaseContext.RemoveRange(guardianEfficiencies);
            return await _databaseContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> DeleteEfficienciesForMatch(string matchId)
        {
            var guardianEfficiencies = _databaseContext.GuardianEfficiencies.Where(eff => eff.MatchId == matchId);

            _databaseContext.RemoveRange(guardianEfficiencies);
            await _databaseContext.SaveChangesAsync();

            return guardianEfficiencies.Select(eff => eff.Id);
        }
    }
}
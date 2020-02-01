using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IGuardianEfficiencyRepository
    {
        GuardianEfficiency GetGuardianEfficiency(string guardianId);
        IEnumerable<double> GetGuardianEfficiencies(string guardianId);
        IEnumerable<GuardianEfficiency> GetGuardianAverageEfficiencies(string guardianId);
        void UpdateGuardianEfficiency(GuardianEfficiency guardianEfficiency);
        void UpdateGuardianMatchEfficiency(IEnumerable<GuardianMatchResult> guardianMatchResults);
        int DeleteGuardianEfficiencies(string guardianId);
        IEnumerable<string> DeleteEfficienciesForMatch(string matchId);
    }
    
    public class GuardianEfficiencyRepository : IGuardianEfficiencyRepository
    {
        private readonly DatabaseContext _databaseContext;

        public GuardianEfficiencyRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public GuardianEfficiency GetGuardianEfficiency(string guardianId)
        {
            var guardianEfficiency = _databaseContext.GuardianEfficiencies.Where(eff => eff.GuardianId == guardianId)
                .OrderByDescending(eff => eff.TimeStamp).FirstOrDefault();

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

        public void UpdateGuardianEfficiency(GuardianEfficiency guardianEfficiency)
        {
            _databaseContext.Add(guardianEfficiency);
            _databaseContext.SaveChanges();
        }

        public void UpdateGuardianMatchEfficiency(IEnumerable<GuardianMatchResult> guardianMatchResults)
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

            _databaseContext.AddRange(efficiencyUpdates);
            _databaseContext.SaveChanges();
        }

        public int DeleteGuardianEfficiencies(string guardianId)
        {
            var guardianEfficiencies = _databaseContext.GuardianEfficiencies.Where(eff => eff.GuardianId == guardianId);

            _databaseContext.RemoveRange(guardianEfficiencies);
            return _databaseContext.SaveChanges();
        }

        public IEnumerable<string> DeleteEfficienciesForMatch(string matchId)
        {
            var guardianEfficiencies = _databaseContext.GuardianEfficiencies.Where(eff => eff.MatchId == matchId);

            _databaseContext.RemoveRange(guardianEfficiencies);
            _databaseContext.SaveChanges();

            return guardianEfficiencies.Select(eff => eff.Id);
        }
    }
}
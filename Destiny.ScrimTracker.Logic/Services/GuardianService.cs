using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IGuardianService
    {
        string CreateGuardian(Guardian guardian);
        IEnumerable<GuardianSnapshot> GetGuardians();
        Guardian GetGuardian(string guardianId);
        Guardian UpdateGuardian(Guardian updatedGuardian);
        string DeleteGuardian(string guardianId);
        IEnumerable<GuardianElo> GetGuardianElo(string guardianId);
        IEnumerable<GuardianEfficiency> GetGuardianEfficiency(string guardianId);
    }
    
    public class GuardianService : IGuardianService
    {
        private readonly IGuardianRepository _guardianRepository;
        private readonly IGuardianEloRepository _guardianEloRepository;
        private readonly IGuardianEfficiencyRepository _guardianEfficiencyRepository;
        private readonly IGuardianMatchResultsRepository _matchResultsRepository;

        public GuardianService(IGuardianRepository guardianRepository, IGuardianEloRepository guardianEloRepository,
            IGuardianEfficiencyRepository guardianEfficiencyRepository, IGuardianMatchResultsRepository matchResultsRepository)
        {
            _guardianRepository = guardianRepository;
            _guardianEloRepository = guardianEloRepository;
            _guardianEfficiencyRepository = guardianEfficiencyRepository;
            _matchResultsRepository = matchResultsRepository;
        }

        public string CreateGuardian(Guardian guardian)
        {
            var guardianId = _guardianRepository.CreateGuardian(guardian);

            var guardianElo = new GuardianElo()
            {
                GuardianId = guardianId,
                Id = $"{ModelIDPrefixes.GuardianElo}_{Guid.NewGuid():N}",
                PreviousElo = 0,
                NewElo = 1200,
            };
            
            var guardianEff = new GuardianEfficiency()
            {
                GuardianId = guardianId,
                Id = $"{ModelIDPrefixes.GuardianEfficiency}_{Guid.NewGuid():N}",
                PreviousEfficiency = 0.0,
                NewEfficiency = 0.0,
                
            };

            _guardianEloRepository.UpdateGuardianElo(guardianElo);
            _guardianEfficiencyRepository.UpdateGuardianEfficiency(guardianEff);
            
            return guardianId;
        }

        public IEnumerable<GuardianSnapshot> GetGuardians()
        {
            var guardians = _guardianRepository.GetAllGuardians();

            var guardianSnapshots = new List<GuardianSnapshot>();
            foreach (var guardian in guardians)
            {
                var guardianElo = _guardianEloRepository.GetGuardianElo(guardian.Id);
                var guardianEff = _guardianEfficiencyRepository.GetGuardianEfficiency(guardian.Id);
                var guardianSnapshot = new GuardianSnapshot()
                {
                    Guardian = guardian,
                    GuardianElo = guardianElo.NewElo,
                    GuardianEfficiency = guardianEff.NewEfficiency,
                };

                guardianSnapshots.Add(guardianSnapshot);
            }
            
            return guardianSnapshots.OrderByDescending(g => g.GuardianElo);
        }

        public Guardian GetGuardian(string guardianId)
        {
            return _guardianRepository.GetGuardian(guardianId);
        }

        public Guardian UpdateGuardian(Guardian updatedGuardian)
        {
            return _guardianRepository.UpdateGuardianStats(updatedGuardian);
        }

        public string DeleteGuardian(string guardianId)
        {
            _guardianEloRepository.DeleteGuardianElos(guardianId);
            _guardianEfficiencyRepository.DeleteGuardianEfficiencies(guardianId);
            _matchResultsRepository.DeleteAllResultsForGuardian(guardianId);
            
            return _guardianRepository.DeleteGuardian(guardianId);
        }

        public IEnumerable<GuardianElo> GetGuardianElo(string guardianId)
        {
            return _guardianEloRepository.GetGuardianElos(guardianId);
        }

        public IEnumerable<GuardianEfficiency> GetGuardianEfficiency(string guardianId)
        {
            return _guardianEfficiencyRepository.GetGuardianAverageEfficiencies(guardianId);
        }
    }
}
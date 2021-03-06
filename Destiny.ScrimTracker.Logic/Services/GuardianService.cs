using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IGuardianService
    {
        Task<string> CreateGuardian(Guardian guardian);
        Task<IEnumerable<GuardianSnapshot>> GetGuardians();
        Task<Guardian> GetGuardian(string guardianId);
        Task<Guardian> UpdateGuardian(Guardian updatedGuardian);
        Task<string> DeleteGuardian(string guardianId);
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

        public async Task<string> CreateGuardian(Guardian guardian)
        {
            var guardianId = await _guardianRepository.CreateGuardian(guardian);

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

            await _guardianEloRepository.UpdateGuardianElo(guardianElo);
            await _guardianEfficiencyRepository.UpdateGuardianEfficiency(guardianEff);
            
            return guardianId;
        }

        public async Task<IEnumerable<GuardianSnapshot>> GetGuardians()
        {
            var guardians = await _guardianRepository.GetAllGuardians();

            var guardianSnapshots = new List<GuardianSnapshot>();
            foreach (var guardian in guardians)
            {
                var guardianElo = await _guardianEloRepository.GetGuardianElo(guardian.Id);
                var guardianEff = _guardianEfficiencyRepository.GetGuardianEfficiency(guardian.Id);
                var guardianSnapshot = new GuardianSnapshot()
                {
                    Guardian = guardian,
                    GuardianElo = guardianElo?.NewElo ?? 1200,
                    GuardianEfficiency = guardianEff?.NewEfficiency ?? 0.0,
                };

                guardianSnapshots.Add(guardianSnapshot);
            }
            
            return guardianSnapshots.OrderByDescending(g => g.GuardianElo);
        }

        public async Task<Guardian> GetGuardian(string guardianId)
        {
            return await _guardianRepository.GetGuardian(guardianId);
        }

        public async Task<Guardian> UpdateGuardian(Guardian updatedGuardian)
        {
            return await _guardianRepository.UpdateGuardianStats(updatedGuardian);
        }

        public async Task<string> DeleteGuardian(string guardianId)
        {
            await _guardianEloRepository.DeleteGuardianElos(guardianId);
            await _guardianEfficiencyRepository.DeleteGuardianEfficiencies(guardianId);
            await _matchResultsRepository.DeleteAllResultsForGuardian(guardianId);
            
            return await _guardianRepository.DeleteGuardian(guardianId);
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
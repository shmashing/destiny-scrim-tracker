using System.Collections;
using System.Collections.Generic;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IGuardianService
    {
        string CreateGuardian(Guardian guardian);
        IEnumerable<Guardian> GetGuardians();
        Guardian GetGuardian(string guardianId);
        Guardian UpdateGuardian(Guardian updatedGuardian);
        string DeleteGuardian(string guardianId);
    }
    
    public class GuardianService : IGuardianService
    {
        private readonly IGuardianRepository _guardianRepository;

        public GuardianService(IGuardianRepository guardianRepository)
        {
            _guardianRepository = guardianRepository;
        }

        public string CreateGuardian(Guardian guardian)
        {
            return _guardianRepository.CreateGuardian(guardian);
        }

        public IEnumerable<Guardian> GetGuardians()
        {
            return _guardianRepository.GetAllGuardians();
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
            return _guardianRepository.DeleteGuardian(guardianId);
        }
    }
}
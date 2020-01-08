using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IGuardianRepository
    {
        string CreateGuardian(Guardian guardian);
        IEnumerable<Guardian> GetAllGuardians();
        Guardian GetGuardian(string guardianId);
        Guardian UpdateGuardianStats(Guardian updatedGuardian);
        string DeleteGuardian(string guardianId);
    }
    
    public class GuardianRepository : IGuardianRepository
    {
        private readonly GuardianContext _guardianContext;

        public GuardianRepository(GuardianContext guardianContext)
        {
            _guardianContext = guardianContext;
        }

        public string CreateGuardian(Guardian guardian)
        {
            using (var context = _guardianContext)
            {
                context.Add(guardian);
                context.SaveChanges();
            }

            return guardian.Id;
        }

        public IEnumerable<Guardian> GetAllGuardians()
        {
            using (var context = _guardianContext)
            {
                var guardians =  context.Guardians.ToList();
                return guardians;
            }
        }

        public Guardian GetGuardian(string guardianId)
        {
            using (var context = _guardianContext)
            {
                var guardian = context.Guardians.FirstOrDefault(guardian => guardian.Id == guardianId);
                return guardian;
            }
        }

        public Guardian UpdateGuardianStats(Guardian updatedGuardian)
        {
            Guardian guardian;
            using (var context = _guardianContext)
            {
                guardian = context.Guardians.FirstOrDefault(g => g.Id == updatedGuardian.Id);

                if (guardian == null)
                {
                    Console.WriteLine($"Record Not Found with Id {updatedGuardian.Id}");
                }
                
                guardian.LifetimeDeaths += updatedGuardian.LifetimeDeaths;
                guardian.LifetimeKills += updatedGuardian.LifetimeKills;
                
                context.Update(guardian);
                context.SaveChanges();
            }

            return guardian;
        }

        public string DeleteGuardian(string guardianId)
        {
            using (var context = _guardianContext)
            {
                var guardian = context.Guardians.FirstOrDefault(g => g.Id == guardianId);

                if (guardian == null)
                {
                    return "Record not found";
                }

                context.Remove(guardian);
                context.SaveChanges();
            }

            return guardianId;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Adapters;
using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IGuardianRepository
    {
        Task<string> CreateGuardian(Guardian guardian);
        Task<IEnumerable<Guardian>> GetAllGuardians();
        Task<Guardian> GetGuardian(string guardianId);
        Task<string> GetGuardianId(string gamerTag);
        Task<Guardian> UpdateGuardianStats(Guardian updatedGuardian);
        Task<Guardian> UpdateGuardianStats(string guardianId, int kills, int deaths);
        Task<Guardian> UpdateGuardianEloModifier(string guardianId, EloModifier newModifier);
        Task<string> DeleteGuardian(string guardianId); 
    }
    
    public class GuardianRepository : IGuardianRepository
    {
        private readonly DatabaseContext _databaseContext;

        public GuardianRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<string> CreateGuardian(Guardian guardian)
        {
            await _databaseContext.AddAsync(guardian);
            await _databaseContext.SaveChangesAsync();

            return guardian.Id;
        }

        public async Task<IEnumerable<Guardian>> GetAllGuardians()
        {
            
            var guardians = await _databaseContext.Guardians.ToListAsync();
            return guardians;
            
        }

        public async Task<Guardian> GetGuardian(string guardianId)
        {

            var guardian = await _databaseContext.Guardians.FirstOrDefaultAsync(guardian => guardian.Id == guardianId);
            return guardian;
            
        }

        public async Task<string> GetGuardianId(string gamerTag)
        {
            var guardian = await _databaseContext.Guardians.FirstOrDefaultAsync(g => g.GamerTag == gamerTag);
            
            return guardian.Id;
        }

        public async Task<Guardian> UpdateGuardianStats(Guardian updatedGuardian)
        {
            var guardian = await _databaseContext.Guardians.FirstOrDefaultAsync(g => g.Id == updatedGuardian.Id);

            if (guardian == null)
            {
                Console.WriteLine($"Record Not Found with Id {updatedGuardian.Id}");
            }
                
            guardian.LifetimeDeaths += updatedGuardian.LifetimeDeaths;
            guardian.LifetimeKills += updatedGuardian.LifetimeKills;
                
            _databaseContext.Update(guardian);
            await _databaseContext.SaveChangesAsync();
            
            return guardian;
        }

        public async Task<Guardian> UpdateGuardianStats(string guardianId, int kills, int deaths)
        {
            var guardian = await _databaseContext.Guardians.FirstOrDefaultAsync(g => g.Id == guardianId);

            guardian.LifetimeDeaths += deaths;
            guardian.LifetimeKills += kills;

            var matchCount = await _databaseContext.GuardianMatchResults.CountAsync(res => res.GuardianId == guardianId);

            if (matchCount >= 30 && guardian.EloModifier == EloModifier.NewGuardian)
            {
                guardian.EloModifier = EloModifier.EstablishedGuardian;
            }

            _databaseContext.Update(guardian);
            await _databaseContext.SaveChangesAsync();

            return guardian;
        }

        public async Task<Guardian> UpdateGuardianEloModifier(string guardianId, EloModifier newModifier)
        {
            var guardian = await _databaseContext.Guardians.FirstOrDefaultAsync(g => g.Id == guardianId);

            guardian.EloModifier = newModifier;

            _databaseContext.Update(guardian);
            await _databaseContext.SaveChangesAsync();

            return guardian;
        }

        public async Task<string> DeleteGuardian(string guardianId)
        {
            var guardian = await _databaseContext.Guardians.FirstOrDefaultAsync(g => g.Id == guardianId);

            if (guardian == null)
            {
                return "Record not found";
            }

            _databaseContext.Remove(guardian);
            await _databaseContext.SaveChangesAsync();

            return guardianId;
        }
    }
}
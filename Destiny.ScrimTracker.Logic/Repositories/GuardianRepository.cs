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
        string GetGuardianId(string gamerTag);
        Guardian UpdateGuardianStats(Guardian updatedGuardian);
        Guardian UpdateGuardianStats(string guardianId, int kills, int deaths);
        Guardian UpdateGuardianEloModifier(string guardianId, EloModifier newModifier);
        string DeleteGuardian(string guardianId);
        void CalculateGuardianElo(Guardian guardian, bool isWinner);
    }
    
    public class GuardianRepository : IGuardianRepository
    {
        private readonly DatabaseContext _databaseContext;

        public GuardianRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public string CreateGuardian(Guardian guardian)
        {
            _databaseContext.Add(guardian);
            _databaseContext.SaveChanges();

            return guardian.Id;
        }

        public IEnumerable<Guardian> GetAllGuardians()
        {
            
            var guardians =  _databaseContext.Guardians.ToList();
            return guardians;
            
        }

        public Guardian GetGuardian(string guardianId)
        {

            var guardian = _databaseContext.Guardians.FirstOrDefault(guardian => guardian.Id == guardianId);
            return guardian;
            
        }

        public string GetGuardianId(string gamerTag)
        {
            var guardianId = _databaseContext.Guardians.FirstOrDefault(g => g.GamerTag == gamerTag).Id;
            return guardianId;
        }

        public Guardian UpdateGuardianStats(Guardian updatedGuardian)
        {
            var guardian = _databaseContext.Guardians.FirstOrDefault(g => g.Id == updatedGuardian.Id);

            if (guardian == null)
            {
                Console.WriteLine($"Record Not Found with Id {updatedGuardian.Id}");
            }
                
            guardian.LifetimeDeaths += updatedGuardian.LifetimeDeaths;
            guardian.LifetimeKills += updatedGuardian.LifetimeKills;
                
            _databaseContext.Update(guardian);
            _databaseContext.SaveChanges();
            
            return guardian;
        }

        public Guardian UpdateGuardianStats(string guardianId, int kills, int deaths)
        {
            var guardian = _databaseContext.Guardians.FirstOrDefault(g => g.Id == guardianId);

            guardian.LifetimeDeaths += deaths;
            guardian.LifetimeKills += kills;

            var matchCount = _databaseContext.GuardianMatchResults.Count(res => res.GuardianId == guardianId);

            if (matchCount >= 30 && guardian.EloModifier == EloModifier.NewGuardian)
            {
                guardian.EloModifier = EloModifier.EstablishedGuardian;
            }

            _databaseContext.Update(guardian);
            _databaseContext.SaveChanges();

            return guardian;
        }

        public Guardian UpdateGuardianEloModifier(string guardianId, EloModifier newModifier)
        {
            var guardian = _databaseContext.Guardians.FirstOrDefault(g => g.Id == guardianId);

            guardian.EloModifier = newModifier;

            _databaseContext.Update(guardian);
            _databaseContext.SaveChanges();

            return guardian;
        }

        public string DeleteGuardian(string guardianId)
        {
            var guardian = _databaseContext.Guardians.FirstOrDefault(g => g.Id == guardianId);

            if (guardian == null)
            {
                return "Record not found";
            }

            _databaseContext.Remove(guardian);
            _databaseContext.SaveChanges();

            return guardianId;
        }

        public void CalculateGuardianElo(Guardian guardian, bool isWinner)
        {
            throw new NotImplementedException();
        }
    }
}
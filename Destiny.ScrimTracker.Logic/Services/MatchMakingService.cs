using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;
using FluentAssertions;
using Newtonsoft.Json;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IMatchMakingService
    {
        Task<IEnumerable<MatchMadeTeam>> MatchTeams(IEnumerable<string> guardiansId, int numberOfTeams);
    }
    
    public class MatchMakingService : IMatchMakingService
    {
        private readonly IGuardianRepository _guardianRepository;
        private readonly IGuardianEloRepository _guardianEloRepository;
        private readonly IGuardianEfficiencyRepository _guardianEfficiencyRepository;
        private readonly IMatchMakingRepository _matchMakingRepository;
        
        public MatchMakingService(IGuardianRepository guardianRepository, 
            IGuardianEloRepository guardianEloRepository,
            IGuardianEfficiencyRepository guardianEfficiencyRepository,
            IMatchMakingRepository matchMakingRepository)
        {
            _guardianRepository = guardianRepository;
            _guardianEloRepository = guardianEloRepository;
            _guardianEfficiencyRepository = guardianEfficiencyRepository;
            _matchMakingRepository = matchMakingRepository;
        }
        
        public async Task<IEnumerable<MatchMadeTeam>> MatchTeams(IEnumerable<string> guardianIds, int teamSize)
        {
            var guardians = new List<GuardianSnapshot>();

            foreach (var id in guardianIds)
            {
                var guardian = await _guardianRepository.GetGuardian(id);
                var guardianElo = await _guardianEloRepository.GetGuardianElo(id);
                var guardianEff = _guardianEfficiencyRepository.GetGuardianEfficiency(id);
                
                var snapshot = new GuardianSnapshot()
                {
                    Guardian = guardian,
                    GuardianEfficiency = guardianEff.NewEfficiency,
                    GuardianElo = guardianElo.NewElo
                };
                
                guardians.Add(snapshot);
            }

            var shuffledGuardians = _matchMakingRepository.ShuffleGuardiansList(guardians);
            var teams = _matchMakingRepository.RandomizeTeams(shuffledGuardians, teamSize);

            teams = _matchMakingRepository.Anneal(teams);
            
            return teams;
        }
    }
}
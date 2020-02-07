using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;
using FluentAssertions;
using Newtonsoft.Json;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IMatchMakingService
    {
        IEnumerable<MatchMadeTeam> MatchTeams(IEnumerable<string> guardiansId, int numberOfTeams);
    }
    
    public class MatchMakingService : IMatchMakingService
    {
        private const double _epsilon = 0.10;
        private const double _alpha = 0.999;
        
        private readonly Random _random;
        private readonly IGuardianRepository _guardianRepository;
        private readonly IGuardianEloRepository _guardianEloRepository;
        private readonly IGuardianEfficiencyRepository _guardianEfficiencyRepository;
        
        public MatchMakingService(IGuardianRepository guardianRepository, 
            IGuardianEloRepository guardianEloRepository,
            IGuardianEfficiencyRepository guardianEfficiencyRepository)
        {
            _random = new Random();
            
            _guardianRepository = guardianRepository;
            _guardianEloRepository = guardianEloRepository;
            _guardianEfficiencyRepository = guardianEfficiencyRepository;
        }
        
        public IEnumerable<MatchMadeTeam> MatchTeams(IEnumerable<string> guardianIds, int teamSize)
        {
            var guardians = new List<GuardianSnapshot>();

            foreach (var id in guardianIds)
            {
                var guardian = _guardianRepository.GetGuardian(id);
                var guardianElo = _guardianEloRepository.GetGuardianElo(id).NewElo;
                var guardianEff = _guardianEfficiencyRepository.GetGuardianEfficiency(id).NewEfficiency;
                
                var snapshot = new GuardianSnapshot()
                {
                    Guardian = guardian,
                    GuardianEfficiency = guardianEff,
                    GuardianElo = guardianElo
                };
                
                guardians.Add(snapshot);
            }
            
            var teams = RandomizeTeams(guardians, teamSize);

            teams = Anneal(teams);
            
            return teams;
        }

        private IEnumerable<MatchMadeTeam> RandomizeTeams(IEnumerable<GuardianSnapshot> guardians, int teamSize)
        {
            var numberOfTeams = guardians.Count() / teamSize;
            var guardianArray = ShuffleGuardiansList(guardians);
            
            var teams = new List<MatchMadeTeam>();
            for (var i = 0; i < numberOfTeams; i++)
            {
                var team = new MatchMadeTeam();
                var teamGuardians = new List<GuardianSnapshot>();
                var teamElo = 0.0;
                var teamEfficiency = 0.0;
                
                for (var j = i * teamSize; j < i * teamSize + teamSize; j++)
                {
                    teamGuardians.Add(guardianArray[j]);
                    teamElo += guardianArray[j].GuardianElo;
                    teamEfficiency += guardianArray[j].GuardianEfficiency;
                }

                team.Guardians = teamGuardians;
                team.TeamElo = teamElo / teamSize;
                team.TeamEfficiency = teamEfficiency / teamSize;
                
                teams.Add(team);
            }

            return teams;
        }

        private IEnumerable<MatchMadeTeam> Anneal(IEnumerable<MatchMadeTeam> teams)
        {
            var temperature = 400.0;

            var iterations = 0;
            while (temperature > _epsilon)
            {
                iterations++;

                var (teamIndex1, teamIndex2) = GetRandomTeamIndexesForSwap(teams);
                var nextTeamConfiguration = SwapTwoPlayers(teams, teamIndex1, teamIndex2);
                
                var teamElos = GetTeamElos(teams).ToArray();
                var newTeamElos = GetTeamElos(nextTeamConfiguration).ToArray();
                
                var startingDelta = CalculateDelta(teamElos[teamIndex1], teamElos[teamIndex2]);
                var newConfigurationDelta = CalculateDelta(newTeamElos[teamIndex1], newTeamElos[teamIndex2]);

                var delta = newConfigurationDelta - startingDelta;
                if (delta < 0)
                {
                    Console.WriteLine("Thats a good swap!");
                    teams = KeepNextConfiguration(nextTeamConfiguration);
                }
                else
                {
                    var probabilityToKeepChange = _random.NextDouble();

                    if (probabilityToKeepChange < Math.Exp(-delta / temperature))
                    {
                        Console.WriteLine($"Probability: {probabilityToKeepChange} and exponential: {Math.Exp(-delta / temperature)}");
                        Console.WriteLine("Eh.. probability checks out. Keeping change");
                        teams = KeepNextConfiguration(nextTeamConfiguration);
                    }
                }
                
                temperature *= _alpha;
            }

            Console.WriteLine($"Total Iterations: {iterations}");
            return teams;
        }

        private GuardianSnapshot[] ShuffleGuardiansList(IEnumerable<GuardianSnapshot> guardians)
        {
            var guardiansArray = guardians.ToArray();
            
            for (var i = 0; i < guardiansArray.Length; i++)
            {
                var randomIndex = _random.Next(0, guardiansArray.Length - 1);
                var guardian = guardiansArray[i];

                guardiansArray[i] = guardiansArray[randomIndex];
                guardiansArray[randomIndex] = guardian;
            }

            return guardiansArray;
        }

        private (int teamIndex1, int teamIndex2) GetRandomTeamIndexesForSwap(IEnumerable<MatchMadeTeam> teams)
        {
            var teamIndex1 = _random.Next(0, teams.Count());
            var teamIndex2 = teamIndex1;
            
            while (teamIndex2 == teamIndex1)
            {
                teamIndex2 = _random.Next(0, teams.Count());
            }

            return (teamIndex1, teamIndex2);
        }

        private IEnumerable<MatchMadeTeam> SwapTwoPlayers(IEnumerable<MatchMadeTeam> teams, int teamIndex1, int teamIndex2)
        {
            var nextConfiguration = new MatchMadeTeam[teams.Count()];
            var teamsArray = teams.ToArray();

            for (var i = 0; i < teams.Count(); i++)
            {
                nextConfiguration[i] = new MatchMadeTeam();
                var guardianArray = teamsArray[i].Guardians.ToArray();
                
                var newGuardiansArray = new GuardianSnapshot[guardianArray.Length];
                
                for (var j = 0; j < guardianArray.Length; j++)
                {
                    newGuardiansArray[j] = guardianArray[j];
                }

                nextConfiguration[i].Guardians = newGuardiansArray;
            }

            var guardianIndex1 = _random.Next(0, nextConfiguration[teamIndex1].Guardians.Count());
            var guardianIndex2 = _random.Next(0, nextConfiguration[teamIndex2].Guardians.Count());

            var team1Guardians = nextConfiguration[teamIndex1].Guardians.ToArray();
            var team2Guardians = nextConfiguration[teamIndex2].Guardians.ToArray();
            
            var guardian1 = team1Guardians[guardianIndex1];
            
            team1Guardians[guardianIndex1] = team2Guardians[guardianIndex2];
            team2Guardians[guardianIndex2] = guardian1;

            nextConfiguration[teamIndex1].Guardians = team1Guardians;
            nextConfiguration[teamIndex2].Guardians = team2Guardians;

            foreach (var team in nextConfiguration)
            {
                var teamElo = 0.0;
                var teamEff = 0.0;

                foreach (var guardian in team.Guardians)
                {
                    teamElo += guardian.GuardianElo;
                    teamEff += guardian.GuardianEfficiency;
                }

                teamElo = teamElo / team.Guardians.Count();
                teamEff = teamEff / team.Guardians.Count();

                team.TeamEfficiency = teamEff;
                team.TeamElo = teamElo;
            }
            return nextConfiguration.ToList();
        }

        private IEnumerable<MatchMadeTeam> KeepNextConfiguration(IEnumerable<MatchMadeTeam> nextTeamConfiguration)
        {
            var newTeamConfiguration = new MatchMadeTeam[nextTeamConfiguration.Count()];
            var teamsArray = nextTeamConfiguration.ToArray();

            for (var i = 0; i < nextTeamConfiguration.Count(); i++)
            {
                newTeamConfiguration[i] = new MatchMadeTeam();
                var guardianArray = teamsArray[i].Guardians.ToArray();
                
                var newGuardiansArray = new GuardianSnapshot[guardianArray.Length];
                
                for (var j = 0; j < guardianArray.Length; j++)
                {
                    newGuardiansArray[j] = guardianArray[j];
                }

                newTeamConfiguration[i].TeamEfficiency = teamsArray[i].TeamEfficiency;
                newTeamConfiguration[i].TeamElo = teamsArray[i].TeamElo;
                newTeamConfiguration[i].Guardians = newGuardiansArray;
            }

            return newTeamConfiguration;
        }
        private IEnumerable<double> GetTeamElos(IEnumerable<MatchMadeTeam> teams)
        {
            return teams.Select(t => t.TeamElo);
        }

        private double CalculateDelta(double elo1, double elo2)
        {
            return Math.Abs(elo1 - elo2);
        }
    }
}
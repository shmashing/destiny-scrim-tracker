using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Destiny.ScrimTracker.Logic.Models;

[assembly: InternalsVisibleTo("Destiny.ScrimTracker.Tests")]
namespace Destiny.ScrimTracker.Logic.Repositories
{
    public interface IMatchMakingRepository
    {
        IEnumerable<GuardianSnapshot> ShuffleGuardiansList(IEnumerable<GuardianSnapshot> guardians);
        IEnumerable<MatchMadeTeam> RandomizeTeams(IEnumerable<GuardianSnapshot> guardians, int teamSize);
        IEnumerable<MatchMadeTeam> Anneal(IEnumerable<MatchMadeTeam> teams);
    }
    
    /*
    * This Algorithm is largely informed by this article: https://www.codeproject.com/Articles/13789/Simulated-Annealing-Example-in-C
    *
    * Boiler plate: the matchmaking algorithm randomly sorts all the player evenly into teams. It then begins to iterate
    * randomly swapping two players. If the swap brings the two teams that the players are on, closer in ELO then the
    * swap is kept.
    * If the swap is not we still want to allow for more random settling. This'll allow us to find a solution that is closer
    * to a global minimum rather than a local minimum. We want the probability of accepting a worse trade to diminish as
    * we iterate and with how much worse the trade is. For example, if the differnce in ELO is not much larger, this
    * should have a larger chance of being kept.
    */
    public class MatchMakingRepository : IMatchMakingRepository
    {
        private const double _epsilon = 0.1; // This is the final temperature of our solution
        private const double _alpha = 0.999; // This is the rate at which our solution cools. The closer to 1, the slower it cools.
        
        private readonly Random _random;

        public MatchMakingRepository()
        {
            _random = new Random();
        }
        
        public IEnumerable<GuardianSnapshot> ShuffleGuardiansList(IEnumerable<GuardianSnapshot> guardians)
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
        public IEnumerable<MatchMadeTeam> RandomizeTeams(IEnumerable<GuardianSnapshot> guardians, int teamSize)
        {
            var numberOfTeams = guardians.Count() / teamSize;
            var guardianArray = guardians.ToArray();
            
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
        
        public IEnumerable<MatchMadeTeam> Anneal(IEnumerable<MatchMadeTeam> teams)
        {
            var temperature = 400.0;

            var propCheckedOut = 0;
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
                    teams = KeepNextConfiguration(nextTeamConfiguration);
                }
                else
                {
                    var probabilityToKeepChange = _random.NextDouble();
                    if (probabilityToKeepChange < Math.Exp(-delta / temperature))
                    {
                        propCheckedOut++;
                        teams = KeepNextConfiguration(nextTeamConfiguration);
                    }
                }
                
                temperature *= _alpha;
            }

            Console.WriteLine($"Total iterations: {iterations}");
            Console.WriteLine($"Times probability checked out: {propCheckedOut}");
            return teams;
        }

        internal (int teamIndex1, int teamIndex2) GetRandomTeamIndexesForSwap(IEnumerable<MatchMadeTeam> teams)
        {
            var teamIndex1 = _random.Next(0, teams.Count());
            var teamIndex2 = teamIndex1;
            
            while (teamIndex2 == teamIndex1)
            {
                teamIndex2 = _random.Next(0, teams.Count());
            }

            return (teamIndex1, teamIndex2);
        }

        internal IEnumerable<MatchMadeTeam> SwapTwoPlayers(IEnumerable<MatchMadeTeam> teams, int teamIndex1, int teamIndex2)
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

        internal IEnumerable<MatchMadeTeam> KeepNextConfiguration(IEnumerable<MatchMadeTeam> nextTeamConfiguration)
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
        internal IEnumerable<double> GetTeamElos(IEnumerable<MatchMadeTeam> teams)
        {
            return teams.Select(t => t.TeamElo);
        }

        internal double CalculateDelta(double elo1, double elo2)
        {
            return Math.Abs(elo1 - elo2);
        }
    }
}
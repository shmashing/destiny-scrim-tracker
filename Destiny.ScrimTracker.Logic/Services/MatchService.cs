using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IMatchService
    {
        Task<string> CreateMatch(Match match, IEnumerable<MatchTeam> teams);
        Task<IEnumerable<MatchResults>> GetMatchResults();
        IEnumerable<GuardianMatchResult> GetMatchResultsForGuardian(string guardianId);
        Task<string> DeleteMatch(string matchId);
    }

    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IMatchTeamRepository _matchTeamRepository;
        private readonly IGuardianMatchResultsRepository _matchResultsRepository;
        private readonly IGuardianRepository _guardianRepository;
        private readonly IGuardianEloRepository _guardianEloRepository;
        private readonly IGuardianEfficiencyRepository _guardianEfficiencyRepository;
        
        public MatchService(IMatchRepository matchRepository, 
            IMatchTeamRepository matchTeamRepository, 
            IGuardianMatchResultsRepository matchResultsRepository,
            IGuardianRepository guardianRepository,
            IGuardianEloRepository guardianEloRepository,
            IGuardianEfficiencyRepository guardianEfficiencyRepository)
        {
            _matchRepository = matchRepository;
            _matchTeamRepository = matchTeamRepository;
            _matchResultsRepository = matchResultsRepository;
            _guardianRepository = guardianRepository;
            _guardianEloRepository = guardianEloRepository;
            _guardianEfficiencyRepository = guardianEfficiencyRepository;
        }

        public async Task<string> CreateMatch(Match match, IEnumerable<MatchTeam> teams)
        {
            var matchId = await _matchRepository.CreateMatch(match);
            var allResults = new List<GuardianMatchResult>();
            foreach (var team in teams)
            {
                team.Id = $"{ModelIDPrefixes.MatchTeam}_{Guid.NewGuid():N}";
                team.MatchId = matchId;

                foreach (var results in team.GuardianMatchResults)
                {
                    results.Id = $"{ModelIDPrefixes.GuardianMatchResult}_{Guid.NewGuid():N}";
                    results.MatchId = matchId;
                    results.MatchTeamId = team.Id;
                    results.GuardianId = await _guardianRepository.GetGuardianId(results.GuardianName);

                    await _guardianRepository.UpdateGuardianStats(results.GuardianId, results.Kills, results.Deaths);
                    allResults.Add(results);
                }
            }

            await _guardianEfficiencyRepository.UpdateGuardianMatchEfficiency(allResults);
            await _matchTeamRepository.WriteMatchTeams(teams);
            await _matchResultsRepository.SaveGuardianResults(allResults);

            var winner = GetWinner(teams);
            var losingTeams = teams.Where(team => team.Id != winner.Id);
            
            await CalculateWinningElo(winner, losingTeams);
            await CalculateLosingElos(winner, losingTeams);
            
            return matchId;
        }

        public async Task<IEnumerable<MatchResults>> GetMatchResults()
        {
            var matches = _matchRepository.GetAllMatches();
            var matchResults = new List<MatchResults>();
            foreach (var match in matches)
            {
                var teams = await _matchTeamRepository.GetTeamsForMatch(match.Id);

                foreach (var team in teams)
                {
                    team.GuardianMatchResults = _matchResultsRepository.GetGuardianMatchResults(match.Id, team.Id);
                }
                
                var matchResult = new MatchResults()
                {
                    MatchType = match.MatchType,
                    Teams = teams,
                    MatchId = match.Id
                };
                
                matchResults.Add(matchResult);
            }
            
            return matchResults;
        }

        public IEnumerable<GuardianMatchResult> GetMatchResultsForGuardian(string guardianId)
        {
            var matches = _matchResultsRepository.GetMatchResultsForGuardian(guardianId);
            return matches;
        }

        public async Task<string> DeleteMatch(string matchId)
        {
            await _matchResultsRepository.DeleteGuardianResults(matchId);
            await _matchTeamRepository.DeleteTeamsForMatch(matchId);
            await _guardianEfficiencyRepository.DeleteEfficienciesForMatch(matchId);
            await _guardianEloRepository.DeleteEloResultForMatch(matchId);
            
            var match = await _matchRepository.DeleteMatch(matchId);
            return match;
        }

        private MatchTeam GetWinner(IEnumerable<MatchTeam> teams)
        {
            return teams.OrderByDescending(team => team.TeamScore).FirstOrDefault();
        }

        private async Task CalculateWinningElo(MatchTeam team, IEnumerable<MatchTeam> losingTeams)
        {
            var winningGuardians = await GetGuardiansForTeam(team);
            var teamElo = CalculateTeamAverageElo(winningGuardians);

            var losingTeamElos = new List<double>();
            foreach (var losingTeam in losingTeams)
            {
                var guardiansForTeam = await GetGuardiansForTeam(losingTeam);
                var losingTeamElo = CalculateTeamAverageElo(guardiansForTeam);

                losingTeamElos.Add(losingTeamElo);
            }

            var averageLosingTeamElo = losingTeamElos.Average();

            var eloDifference = (averageLosingTeamElo - teamElo)/400;
            var invertedExpectedOutcome = Math.Pow(10, eloDifference) + 1;
            var expectedOutcome = 1 / invertedExpectedOutcome;

            foreach (var guardian in winningGuardians)
            {
                var guardianResult = team.GuardianMatchResults.FirstOrDefault(res => res.GuardianId == guardian.Id);
                var newGuardianElo = _guardianEloRepository.CalculateGuardianElo(guardian, true, 
                    guardianResult.Efficiency, expectedOutcome, 1, team.MatchId);
                await _guardianEloRepository.UpdateGuardianElo(newGuardianElo);

                if (newGuardianElo.NewElo >= 2100)
                {
                    await _guardianRepository.UpdateGuardianEloModifier(guardian.Id, EloModifier.HighRankingGuardian);
                }
            }
        }

        private async Task CalculateLosingElos(MatchTeam winningTeam, IEnumerable<MatchTeam> losingTeams)
        {
            var winningTeamGuardians = await GetGuardiansForTeam(winningTeam);
            var winningTeamElo = CalculateTeamAverageElo(winningTeamGuardians);
            
            foreach (var team in losingTeams)
            {
                var guardians = await GetGuardiansForTeam(team);
                var teamElo = CalculateTeamAverageElo(guardians);

                var eloDifference = (winningTeamElo - teamElo) / 400;
                var invertedExpectedOutcome = Math.Pow(10, eloDifference) + 1;
                var expectedOutcome = 1 / invertedExpectedOutcome;

                foreach (var guardian in guardians)
                {
                    var guardianResult = team.GuardianMatchResults.FirstOrDefault(res => res.GuardianId == guardian.Id);
                    var newGuardianElo = _guardianEloRepository.CalculateGuardianElo(guardian, false,
                        guardianResult.Efficiency, expectedOutcome, 0, team.MatchId);
                    await _guardianEloRepository.UpdateGuardianElo(newGuardianElo);
                }
            }
        }

        private async Task<IEnumerable<Guardian>> GetGuardiansForTeam(MatchTeam team)
        {
            var guardians = new List<Guardian>();
            foreach (var result in team.GuardianMatchResults)
            {
                var guardian = await _guardianRepository.GetGuardian(result.GuardianId);
                guardians.Add(guardian);
            }

            return guardians;
        }
        
        private double CalculateTeamAverageElo(IEnumerable<Guardian> guardians)
        {
            double elo = 0;
            foreach (var guardian in guardians)
            {
                elo += _guardianEloRepository.GetGuardianElo(guardian.Id).Result.NewElo;
            }
            
            return elo/guardians.Count();
        }
    }
}
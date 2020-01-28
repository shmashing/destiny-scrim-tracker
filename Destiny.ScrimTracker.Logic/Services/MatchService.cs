using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using Destiny.ScrimTracker.Logic.Models;
using Destiny.ScrimTracker.Logic.Repositories;

namespace Destiny.ScrimTracker.Logic.Services
{
    public interface IMatchService
    {
        string CreateMatch(Match match, IEnumerable<MatchTeam> teams);
        IEnumerable<MatchResults> GetMatchResults();
        string DeleteMatch(string matchId);
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

        public string CreateMatch(Match match, IEnumerable<MatchTeam> teams)
        {
            var matchId = _matchRepository.CreateMatch(match);
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
                    results.GuardianId = _guardianRepository.GetGuardianId(results.GuardianName);

                    _guardianRepository.UpdateGuardianStats(results.GuardianId, results.Kills, results.Deaths);
                    allResults.Add(results);
                }
            }

            _guardianEfficiencyRepository.UpdateGuardianMatchEfficiency(allResults);
            _matchTeamRepository.WriteMatchTeams(teams);
            _matchResultsRepository.SaveGuardianResults(allResults);

            var winner = GetWinner(teams);
            var losingTeams = teams.Where(team => team.Id != winner.Id);
            
            CalculateWinningElo(winner, losingTeams);
            CalculateLosingElos(winner, losingTeams);
            
            return matchId;
        }

        public IEnumerable<MatchResults> GetMatchResults()
        {
            var matches = _matchRepository.GetAllMatches();
            var matchResults = new List<MatchResults>();
            foreach (var match in matches)
            {
                var teams = _matchTeamRepository.GetTeamsForMatch(match.Id);

                foreach (var team in teams)
                {
                    team.GuardianMatchResults = _matchResultsRepository.GetGuardianMatchResults(match.Id, team.Id);
                }
                
                var matchResult = new MatchResults()
                {
                    MatchType = match.MatchType,
                    Teams = teams,
                };
                
                matchResults.Add(matchResult);
            }
            
            return matchResults;
        }

        public string DeleteMatch(string matchId)
        {
            _matchResultsRepository.DeleteGuardianResults(matchId);
            _matchTeamRepository.DeleteTeamsForMatch(matchId);
            _guardianEfficiencyRepository.DeleteEfficienciesForMatch(matchId);
            _guardianEloRepository.DeleteEloResultForMatch(matchId);
            
            var match = _matchRepository.DeleteMatch(matchId);
            return match;
        }

        private MatchTeam GetWinner(IEnumerable<MatchTeam> teams)
        {
            return teams.OrderByDescending(team => team.TeamScore).FirstOrDefault();
        }

        private void CalculateWinningElo(MatchTeam team, IEnumerable<MatchTeam> losingTeams)
        {
            var winningGuardians = GetGuardiansForTeam(team);
            var teamElo = CalculateTeamAverageElo(winningGuardians);

            var losingTeamElos = losingTeams.Select(GetGuardiansForTeam).Select(CalculateTeamAverageElo).ToList();
            var averageLosingTeamElo = losingTeamElos.Average();

            var eloDifference = (averageLosingTeamElo - teamElo)/400;
            var invertedExpectedOutcome = Math.Pow(10, eloDifference) + 1;
            var expectedOutcome = 1 / invertedExpectedOutcome;

            foreach (var guardian in winningGuardians)
            {
                var guardianResult = team.GuardianMatchResults.FirstOrDefault(res => res.GuardianId == guardian.Id);
                var newGuardianElo = _guardianEloRepository.CalculateGuardianElo(guardian, true, 
                    guardianResult.Efficiency, expectedOutcome, 1, team.MatchId);
                _guardianEloRepository.UpdateGuardianElo(newGuardianElo);

                if (newGuardianElo.NewElo >= 2100)
                {
                    _guardianRepository.UpdateGuardianEloModifier(guardian.Id, EloModifier.HighRankingGuardian);
                }
            }
        }

        private void CalculateLosingElos(MatchTeam winningTeam, IEnumerable<MatchTeam> losingTeams)
        {
            var winningTeamGuardians = GetGuardiansForTeam(winningTeam);
            var winningTeamElo = CalculateTeamAverageElo(winningTeamGuardians);
            
            foreach (var team in losingTeams)
            {
                var guardians = GetGuardiansForTeam(team);
                var teamElo = CalculateTeamAverageElo(guardians);

                var eloDifference = (winningTeamElo - teamElo) / 400;
                var invertedExpectedOutcome = Math.Pow(10, eloDifference) + 1;
                var expectedOutcome = 1 / invertedExpectedOutcome;

                foreach (var guardian in guardians)
                {
                    var guardianResult = team.GuardianMatchResults.FirstOrDefault(res => res.GuardianId == guardian.Id);
                    var newGuardianElo = _guardianEloRepository.CalculateGuardianElo(guardian, false,
                        guardianResult.Efficiency, expectedOutcome, 0, team.MatchId);
                    _guardianEloRepository.UpdateGuardianElo(newGuardianElo);
                }
            }
        }

        private IEnumerable<Guardian> GetGuardiansForTeam(MatchTeam team)
        {
            return team.GuardianMatchResults.Select(guardianResult => _guardianRepository.GetGuardian(guardianResult.GuardianId)).ToList();
        }
        
        private double CalculateTeamAverageElo(IEnumerable<Guardian> guardians)
        {
            double elo = 0;
            foreach (var guardian in guardians)
            {
                elo += _guardianEloRepository.GetGuardianElo(guardian.Id).NewElo;
            }
            
            return elo/guardians.Count();
        }
    }
}
using System;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class GuardianMatchResult
    {
        public Guid Id { get; set; }
        public Guid GuardianId { get; set; }
        public Guid MatchId { get; set; }
        public Guid MatchTeamId { get; set; }
        public double Efficiency { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }

        public GuardianMatchResult()
        {
            
        }
    }
}
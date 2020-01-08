using System;
using System.Collections;
using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class MatchTeam
    {
        public Guid Id { get; set; }
        public Guid MatchId { get; set; }
        public IEnumerable<GuardianMatchResult> GuardianResults { get; set; }
        public int TeamScore { get; set; }
    }
}
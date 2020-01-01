using System.Collections;
using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class MatchTeam
    {
        public IEnumerable<GuardianMatchResult> Guardians { get; set; }
        public int TeamScore { get; set; }
    }
}
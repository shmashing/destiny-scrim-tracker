using System.Collections;
using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class GuardianHistory
    {
        public Guardian Guardian { get; set; }
        public IEnumerable<GuardianElo> EloHistory { get; set; }
        public IEnumerable<GuardianEfficiency> EfficiencyHistory { get; set; }
        public int MatchCount { get; set; }
    }
}
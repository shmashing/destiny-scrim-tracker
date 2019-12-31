using System.Collections;
using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class Match
    {
        public IEnumerable<GuardianMatchResult> AlphaTeam { get; set; }
        public IEnumerable<GuardianMatchResult> BravoTeam { get; set; }
        public int AlphaTeamScore { get; set; }
        public int BravoTeamScore { get; set; }
    }
}
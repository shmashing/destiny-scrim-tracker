using System.Collections;
using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class MatchMadeTeam
    {
        public IEnumerable<GuardianSnapshot> Guardians { get; set; }
        public double TeamElo { get; set; }
        public double TeamEfficiency { get; set; }
    }
}
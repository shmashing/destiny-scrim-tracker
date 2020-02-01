using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class MatchResults
    {
        public string MatchId { get; set; }
        public MatchType MatchType { get; set; }
        public IEnumerable<MatchTeam> Teams { get; set; }
    }
}
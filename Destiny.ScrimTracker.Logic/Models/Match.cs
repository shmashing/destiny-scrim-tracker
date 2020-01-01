using System;
using System.Collections;
using System.Collections.Generic;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class Match
    {
        public IEnumerable<MatchTeam> Teams { get; set; }
        public DateTime TimeStamp { get; set; }
        public MatchType MatchType { get; set; }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Destiny.ScrimTracker.App.Requests
{
    public class CreateMatchFormModel
    {
        public IEnumerable<Team> Teams { get; set; }
        public MatchType MatchType { get; set; }
    }

    public class Team
    {
        [Range(0, Int32.MaxValue)]
        public int TeamScore { get; set; }
        public IEnumerable<MatchResult> MatchResults { get; set; }
    }

    public class MatchResult
    {
        public string GuardianName { get; set; }
        public int Kills { get; set; }
        public int Assists { get; set; }
        public int Deaths { get; set; }
    }
}
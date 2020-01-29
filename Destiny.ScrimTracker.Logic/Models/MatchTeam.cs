using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Destiny.ScrimTracker.Logic.Models
{
    [Table("match_teams", Schema = "scrims_tracker")]
    public class MatchTeam
    {
        public string Id { get; set; }
        public string MatchId { get; set; }
        public IEnumerable<GuardianMatchResult> GuardianMatchResults { get; set; }
        public string Name { get; set; }
        public int TeamScore { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Destiny.ScrimTracker.Logic.Models
{
    [Table("guardian_match_results", Schema = "scrims_tracker")]
    public class GuardianMatchResult
    {
        public string Id { get; set; }
        public string GuardianId { get; set; }
        public string GuardianName { get; set; }
        public string MatchId { get; set; }
        public string MatchTeamId { get; set; }
        public double Efficiency { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
    }
}
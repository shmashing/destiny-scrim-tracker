using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Destiny.ScrimTracker.Logic.Models
{
    [Table("guardian_elos", Schema = "scrims_tracker")]
    public class GuardianElo
    {
        public string Id { get; set; }
        public string? MatchId { get; set; }
        public string GuardianId { get; set; }
        public double PreviousElo { get; set; }
        public double NewElo { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Destiny.ScrimTracker.Logic.Models
{
    [Table("guardian_efficiencies", Schema = "scrims_tracker")]
    public class GuardianEfficiency
    {
        public string Id { get; set; }
        public string? MatchId { get; set; }
        public string GuardianId { get; set; }
        public double PreviousEfficiency { get; set; }
        public double NewEfficiency { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
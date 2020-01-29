using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Destiny.ScrimTracker.Logic.Models
{
    [Table("matches", Schema = "scrims_tracker")]
    public class Match
    {
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public MatchType MatchType { get; set; }
    }
}
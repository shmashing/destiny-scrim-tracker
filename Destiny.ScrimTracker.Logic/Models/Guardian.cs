using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;

namespace Destiny.ScrimTracker.Logic.Models
{
    [Table("guardians", Schema = "scrims_tracker")]
    public class Guardian
    {
        public string Id { get; set; }
        [Required]
        public string GamerTag { get; set; }
        public int LifetimeKills { get; set; }
        public int LifetimeDeaths { get; set; }
    }
}
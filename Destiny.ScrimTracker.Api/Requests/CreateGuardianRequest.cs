using System;
using System.ComponentModel.DataAnnotations;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.Api.Requests
{
    public class CreateGuardianRequest
    {
        [Required]
        public string GamerTag { get; set; }

        public Guardian ToGuardian()
        {
            return new Guardian()
            {
                Id = $"{ModelIDPrefixes.Guardian}_{Guid.NewGuid():N}",
                GamerTag = this.GamerTag,
                EloModifier = EloModifier.NewGuardian
            };
        }
    }
}
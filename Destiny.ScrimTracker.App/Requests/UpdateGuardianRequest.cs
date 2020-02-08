using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.App.Requests
{
    public class UpdateGuardianRequest
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }

        public Guardian ToGuardian(string guradianId)
        {
            return new Guardian()
            {
                Id = guradianId,
                LifetimeDeaths = Deaths,
                LifetimeKills = Kills
            };
        }
    }
}
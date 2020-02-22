using System.Collections;
using System.Collections.Generic;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.Api
{
    public class GuardianResponse
    {
        public Guardian Guardian { get; set; }
        public IEnumerable<GuardianElo> GuardianElo { get; set; }
    }
}
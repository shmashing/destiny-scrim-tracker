using System.Collections;
using System.Collections.Generic;
using Destiny.ScrimTracker.Logic.Models;

namespace Destiny.ScrimTracker.App
{
    public class AllGuardiansResponse
    {
        public IEnumerable<GuardianSnapshot> GuardianSnapshots { get; set; }
    }
}
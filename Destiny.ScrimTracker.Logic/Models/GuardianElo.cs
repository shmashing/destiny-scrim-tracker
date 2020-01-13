using System;

namespace Destiny.ScrimTracker.Logic.Models
{
    public class GuardianElo
    {
        public Guid Id { get; set; }
        public int MatchId { get; set; }
        public int GuardianId { get; set; }
        public int PreviousElo { get; set; }
        public int NewElo { get; set; }
    }
}
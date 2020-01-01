namespace Destiny.ScrimTracker.Logic.Models
{
    public class GuardianEfficiency
    {
        public int MatchId { get; set; }
        public int GuardianId { get; set; }
        public double PreviousEfficiency { get; set; }
        public double NewEfficiency { get; set; }
    }
}
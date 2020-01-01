namespace Destiny.ScrimTracker.Logic.Models
{
    public class GuardianMatchResult
    {
        public int GuardianId { get; set; }
        public double MatchEfficiency { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
    }
}
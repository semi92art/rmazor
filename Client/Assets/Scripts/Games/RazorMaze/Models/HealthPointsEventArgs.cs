namespace Games.RazorMaze.Models
{
    public class HealthPointsEventArgs : System.EventArgs
    {
        public long HealthPoints { get; }
        public HealthPointsEventArgs(long _HealthPoints) => HealthPoints = _HealthPoints;
    }
}
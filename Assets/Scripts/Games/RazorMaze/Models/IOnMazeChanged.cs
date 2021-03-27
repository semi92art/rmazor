namespace Games.RazorMaze.Models
{
    public interface IOnMazeChanged
    {
        void OnMazeChanged(MazeInfo _Info);
    }
}
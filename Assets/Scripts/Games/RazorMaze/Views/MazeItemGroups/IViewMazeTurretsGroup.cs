using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTurretsGroup : IInit
    {
        void OnTurretShoot(TurretShotEventArgs _Args);
    }
}
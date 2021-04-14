using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTurretsGroup : IInit
    {
        void OnTurretShoot(TurretShotEventArgs _Args);
    }
}
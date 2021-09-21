using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTurretsGroup : IInit, IOnLevelStageChanged, IMazeItemTypes
    {
        void OnTurretShoot(TurretShotEventArgs _Args);
    }
}
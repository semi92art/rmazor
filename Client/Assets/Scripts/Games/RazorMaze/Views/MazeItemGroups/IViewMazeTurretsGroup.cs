using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTurretsGroup : IInit, IOnLevelStageChanged, IMazeItemTypes, IOnBackgroundColorChanged
    {
        void OnTurretShoot(TurretShotEventArgs _Args);
    }
}
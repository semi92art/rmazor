using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeMovingItemsGroup : IInit
    {
        void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args);
    }
}
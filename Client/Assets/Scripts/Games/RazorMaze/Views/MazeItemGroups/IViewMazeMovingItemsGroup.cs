using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeMovingItemsGroup : IViewMazeItemGroup
    {
        void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args);
    }
}
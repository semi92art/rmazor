using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTrapsIncreasingItemsGroup : IInit
    {
        void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args);
    }
}
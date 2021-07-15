using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTrapsIncreasingItemsGroup : IInit
    {
        void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args);
    }
}
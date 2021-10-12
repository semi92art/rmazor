using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTrapsIncItemsGroup :
        IInit,
        IOnLevelStageChanged,
        IViewMazeItemGroup
    {
        void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args);
    }
}
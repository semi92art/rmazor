using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTrapsReactItemsGroup
    {
        void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args);
    }
}
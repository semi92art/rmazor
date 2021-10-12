using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTrapsReactItemsGroup : 
        IInit,
        IOnLevelStageChanged, 
        IViewMazeItemGroup
    {
        void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args);
    }
}
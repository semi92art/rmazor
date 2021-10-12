using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeSpringboardItemsGroup :
        IOnLevelStageChanged, 
        IViewMazeItemGroup
    {
        void OnSpringboardEvent(SpringboardEventArgs _Args);
    }
}
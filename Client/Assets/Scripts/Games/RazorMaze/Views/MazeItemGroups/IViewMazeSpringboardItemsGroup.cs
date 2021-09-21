using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeSpringboardItemsGroup : IOnLevelStageChanged, IMazeItemTypes
    {
        void OnSpringboardEvent(SpringboardEventArgs _Args);
    }
}
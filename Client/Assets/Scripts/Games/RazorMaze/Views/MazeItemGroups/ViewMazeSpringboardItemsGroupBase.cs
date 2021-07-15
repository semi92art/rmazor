using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeSpringboardItemsGroupBase : IViewMazeSpringboardItemsGroup
    {
        public abstract void OnSpringboardEvent(SpringboardEventArgs _Args);
    }
}
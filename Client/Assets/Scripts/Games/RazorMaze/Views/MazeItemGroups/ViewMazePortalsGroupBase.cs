using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazePortalsGroupBase : IViewMazePortalsGroup
    {
        public abstract void OnPortalEvent(PortalEventArgs _Args);
    }
}
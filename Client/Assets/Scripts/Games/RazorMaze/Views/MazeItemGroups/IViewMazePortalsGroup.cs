using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazePortalsGroup : IOnLevelStageChanged, IMazeItemTypes
    {
        void OnPortalEvent(PortalEventArgs _Args);
    }
}
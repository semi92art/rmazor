using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeShredingerBlocksGroup : IOnLevelStageChanged, IMazeItemTypes
    {
        void OnShredingerBlockEvent(ShredingerBlockArgs _Args);
    }
}
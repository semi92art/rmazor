using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeShredingerBlocksGroup : IOnLevelStageChanged, IMazeItemTypes, ICharacterMoveFinished
    {
        void OnShredingerBlockEvent(ShredingerBlockArgs _Args);
    }
}
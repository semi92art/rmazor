using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeShredingerBlocksGroup :
        IOnLevelStageChanged,
        IViewMazeItemGroup,
        ICharacterMoveFinished
    {
        void OnShredingerBlockEvent(ShredingerBlockArgs _Args);
    }
}
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacter :
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnBackgroundColorChanged,
        IAppear
    {
        void OnRotationAfterFinished(MazeRotationEventArgs _Args);
    }
}
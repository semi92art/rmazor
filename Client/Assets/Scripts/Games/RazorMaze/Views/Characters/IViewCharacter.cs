using Entities;
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
        IAppear
    {
        void OnRotationAfterFinished(MazeRotationEventArgs _Args);
        void OnAllPathProceed(V2Int _LastPath);
    }
}
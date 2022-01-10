using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacterTail :
        IActivated,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnLevelStageChanged
    {
        void OnAllPathProceed(V2Int _LastPath);
        void ShowTail(CharacterMoveEventArgsBase _Args);
        void HideTail(CharacterMovingFinishedEventArgs _Args = null);
    }
}
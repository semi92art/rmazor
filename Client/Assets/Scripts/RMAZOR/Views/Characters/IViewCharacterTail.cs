using Common.Entities;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using SpawnPools;

namespace RMAZOR.Views.Characters
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
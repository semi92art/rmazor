using Common.Entities;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;

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
using Common.Entities;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using SpawnPools;

namespace RMAZOR.Views.Characters
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
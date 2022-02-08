using Common.Entities;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;

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
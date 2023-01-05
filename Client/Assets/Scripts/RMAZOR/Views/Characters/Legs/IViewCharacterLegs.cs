using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;

namespace RMAZOR.Views.Characters.Legs
{
    public interface IViewCharacterLegs :
        IInit,
        IActivated,
        ICharacterMoveStarted,
        ICharacterMoveContinued, 
        ICharacterMoveFinished, 
        IOnLevelStageChanged,
        IOnPathCompleted,
        IAppear
    {
        void OnRotationFinished(MazeRotationEventArgs _Args);
    }
}
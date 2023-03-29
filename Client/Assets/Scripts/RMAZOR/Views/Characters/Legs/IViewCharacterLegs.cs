using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;
using RMAZOR.Views.Rotation;

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
        IMazeRotationFinished,
        IAppear {
   }
}
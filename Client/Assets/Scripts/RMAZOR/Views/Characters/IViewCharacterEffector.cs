using Common;
using Common.SpawnPools;
using RMAZOR.Models.ItemProceeders.Additional;

namespace RMAZOR.Views.Characters
{
    public interface IViewCharacterEffector :
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnPathCompleted { }
}
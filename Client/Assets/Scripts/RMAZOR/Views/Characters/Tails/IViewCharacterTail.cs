using System;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models.ItemProceeders.Additional;

namespace RMAZOR.Views.Characters.Tails
{
    public interface IViewCharacterTail :
        IInit,
        IActivated,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnLevelStageChanged,
        IOnPathCompleted
    {
        Func<ViewCharacterInfo> GetCharacterObjects { get; set; }
    }


}
using Common;
using Common.Entities;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;

namespace RMAZOR.Views.Characters
{
    public interface IViewCharacter :
        IInit,
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IOnPathCompleted,
        IAppear
    {
        ViewCharacterInfo GetObjects();
        void              OnRotationFinished(MazeRotationEventArgs _Args);
    }
}
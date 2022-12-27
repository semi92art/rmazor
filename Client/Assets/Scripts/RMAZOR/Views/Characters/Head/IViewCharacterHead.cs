using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHead :
        IInit,
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveFinished,
        IOnPathCompleted,
        IAppear
    {
        Transform    Transform { get; }
        Collider2D[] Colliders { get; }
        void         OnRotationFinished(MazeRotationEventArgs _Args);
    }
}
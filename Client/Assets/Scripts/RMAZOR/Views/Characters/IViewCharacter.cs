using Common.Entities;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;
using UnityEngine;

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
        Transform    Transform { get; }
        Collider2D[] Colliders { get; }
        void         OnRotationFinished(MazeRotationEventArgs _Args);
        void         OnAllPathProceed(V2Int                   _LastPath);
    }
}
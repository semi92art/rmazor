using System;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.CoordinateConverters;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public abstract class ViewCharacterBase : IViewCharacter
    {
        #region constructor

        protected ICoordinateConverterRmazor CoordinateConverter { get; }
        protected IModelGame Model { get; }

        protected IContainersGetter ContainersGetter { get; }
        protected IViewMazeCommon ViewMazeCommon { get; }

        protected ViewCharacterBase(
            ICoordinateConverterRmazor _CoordinateConverter, 
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon)
        {
            CoordinateConverter = _CoordinateConverter;
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            ViewMazeCommon = _ViewMazeCommon;
        }

        #endregion
        
        #region api

        public abstract EAppearingState AppearingState { get; }
        public virtual  bool            Activated      { get; set; }
        public abstract Transform       Transform      { get; }
        public abstract Collider2D[]    Colliders      { get; }
        
        public abstract void            OnRotationFinished(MazeRotationEventArgs                   _Args);
        public abstract void            OnAllPathProceed(V2Int                                     _LastPath);
        public abstract void            OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args);
        public abstract void            OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args);
        public abstract void            OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args);
        public abstract void            OnLevelStageChanged(LevelStageArgs                         _Args);
        public abstract void            Appear(bool                                                _Appear);

        #endregion
        
        #region nonpublic methods
        
        protected void SetPosition(Vector2 _Position) =>
            ContainersGetter.GetContainer(ContainerNames.Character).localPosition = _Position;
        
        #endregion

    }
}
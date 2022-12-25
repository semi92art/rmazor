using Common.Entities;
using Common.Helpers;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.Characters
{
    public abstract class ViewCharacterBase : InitBase, IViewCharacter
    {
        #region inject

        protected ICoordinateConverter CoordinateConverter { get; }
        protected IModelGame           Model               { get; }
        protected IContainersGetter    ContainersGetter    { get; }

        protected ViewCharacterBase(
            ICoordinateConverter _CoordinateConverter,
            IModelGame           _Model,
            IContainersGetter    _ContainersGetter)
        {
            CoordinateConverter = _CoordinateConverter;
            Model               = _Model;
            ContainersGetter    = _ContainersGetter;
        }

        #endregion
        
        #region api

        public abstract EAppearingState   AppearingState { get; }
        public virtual  bool              Activated      { get; set; }
        
        public abstract ViewCharacterInfo GetObjects();
        public abstract void              OnRotationFinished(MazeRotationEventArgs                   _Args);
        public abstract void              OnPathCompleted(V2Int                                     _LastPath);
        public abstract void              OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args);
        public abstract void              OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args);
        public abstract void              OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args);
        public abstract void              OnLevelStageChanged(LevelStageArgs                         _Args);
        public abstract void              Appear(bool                                                _Appear);

        #endregion
    }
}
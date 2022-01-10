using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;

namespace Games.RazorMaze.Views.Characters
{
    public abstract class ViewCharacterBase : IViewCharacter
    {
        #region constructor

        protected IMazeCoordinateConverter CoordinateConverter { get; }
        protected IModelGame Model { get; }

        protected IContainersGetter ContainersGetter { get; }
        protected IViewMazeCommon ViewMazeCommon { get; }

        protected ViewCharacterBase(
            IMazeCoordinateConverter _CoordinateConverter, 
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
        public abstract void            OnRotationAfterFinished(MazeRotationEventArgs _Args);
        public abstract void            OnAllPathProceed(V2Int _LastPath);
        public abstract void            OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args);
        public abstract void            OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args);
        public abstract void            OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args);
        public abstract void            OnLevelStageChanged(LevelStageArgs _Args);
        public abstract void            Appear(bool _Appear);

        #endregion
        
        #region nonpublic methods
        
        protected void SetPosition(Vector2 _Position) =>
            ContainersGetter.GetContainer(ContainerNames.Character).localPosition = _Position;
        
        #endregion
    }
}
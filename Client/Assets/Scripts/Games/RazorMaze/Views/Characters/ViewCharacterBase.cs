﻿using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using UnityEngine;

namespace Games.RazorMaze.Views.Characters
{
    public abstract class ViewCharacterBase : IViewCharacter
    {
        #region constructor

        protected ICoordinateConverter CoordinateConverter { get; }
        protected IModelMazeData ModelMazeData { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected IViewMazeCommon ViewMazeCommon { get; }

        protected ViewCharacterBase(
            ICoordinateConverter _CoordinateConverter, 
            IModelMazeData _ModelMazeData, 
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon)
        {
            CoordinateConverter = _CoordinateConverter;
            ModelMazeData = _ModelMazeData;
            ContainersGetter = _ContainersGetter;
            ViewMazeCommon = _ViewMazeCommon;
        }

        #endregion
        
        
        #region api
        
        public event NoArgsHandler Initialized;
        public virtual bool Activated { get; set; }

        public virtual void Init() => Initialized?.Invoke();
        public abstract void OnMovingStarted(CharacterMovingEventArgs _Args);
        public abstract void OnMoving(CharacterMovingEventArgs _Args);
        public abstract void OnMovingFinished(CharacterMovingEventArgs _Args);
        public abstract void OnRevivalOrDeath(bool _Alive);
        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        public virtual void OnPositionSet(V2Int _Position)
        {
            CoordinateConverter.Init(ModelMazeData.Info.Size);
            SetPosition(CoordinateConverter.ToLocalCharacterPosition(_Position));
        }
        
        #endregion
        
        #region nonpublic methods
        
        protected void SetPosition(Vector2 _Position) =>
            ContainersGetter.CharacterContainer.localPosition = _Position;
        
        #endregion
    }
}
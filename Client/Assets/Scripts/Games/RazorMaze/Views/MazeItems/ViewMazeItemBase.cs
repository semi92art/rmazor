﻿using System;
using System.Collections.Generic;
using DI.Extensions;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems.Props;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public enum EAppearingState
    {
        Appearing,
        Appeared,
        Dissapearing,
        Dissapeared
    }

    public enum EProceedingStage
    {
        Inactive,
        Active,
        ActiveAndWorking
    }

    public abstract class ViewMazeItemBase : IViewMazeItem
    {
        #region nonpublic members
        
        protected abstract object[] DefaultColorShapes { get; }
        protected bool Initialized { get; set; }
        protected bool m_ActivatedInSpawnPool;

        #endregion

        #region inject

        protected ViewSettings ViewSettings { get; }
        protected IModelGame Model { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected IGameTicker GameTicker { get; }
        protected IViewAppearTransitioner Transitioner { get; }
        protected IManagersGetter Managers { get; }


        protected ViewMazeItemBase (
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers)
        {
            ViewSettings = _ViewSettings;
            Model = _Model;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Transitioner = _Transitioner;
            Managers = _Managers;

            GameTicker.Register(this);
        }

        #endregion
        
        #region api
        
        public ViewMazeItemProps Props { get; set; }
        public abstract object Clone();
        
        public virtual bool ActivatedInSpawnPool
        {
            get => m_ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                ActivateShapes(false);
            }
        }
        
        public virtual GameObject Object { get; protected set; }
        
        public virtual EAppearingState AppearingState { get; set; }
        public virtual EProceedingStage ProceedingStage { get; set; }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                case ELevelStage.Finished:
                    ProceedingStage = EProceedingStage.Active;
                    break;
                case ELevelStage.CharacterKilled:
                case ELevelStage.ReadyToStartOrContinue:
                case ELevelStage.StartedOrContinued:
                    ProceedingStage = EProceedingStage.ActiveAndWorking;
                    break;
                case ELevelStage.Paused:
                case ELevelStage.Unloaded:
                    ProceedingStage = EProceedingStage.Inactive;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        public virtual void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            if (!Initialized)
                InitShape();
            UpdateShape();
            Initialized = true;
        }

        public bool Equal(IMazeItemProceedInfo _Info)
        {
            return Props != null && Props.Equals(_Info);
        }
        
        public void SetLocalPosition(Vector2 _Position)
        {
            Object.transform.SetLocalPosXY(_Position);
        }

        public void SetLocalScale(float _Scale)
        {
            Object.transform.localScale = _Scale * Vector3.one;
        }

        public virtual void Appear(bool _Appear)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    OnAppearStart(_Appear);
                    Transitioner.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        GetAppearSets(_Appear),
                        Props.Position,
                        () => OnAppearFinish(_Appear));
                }));
        }

        #endregion
        
        #region nonpublic methods
        
        protected abstract void InitShape();
        protected abstract void UpdateShape();

        protected virtual void OnAppearStart(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                ActivateShapes(true);
        }

        protected virtual Dictionary<object[], Func<Color>> GetAppearSets(bool _Appear)
        {
            return new Dictionary<object[], Func<Color>>
            {
                {DefaultColorShapes, () => DrawingUtils.ColorLines}
            };
        }

        protected virtual void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                ActivateShapes(false);
            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
        }

        protected void ActivateShapes(bool _Activate)
        {
            foreach (var shape in DefaultColorShapes)
            {
                if (shape is ShapeRenderer shapeRenderer && !shapeRenderer.IsNull())
                    shapeRenderer.enabled = _Activate;
                else if (shape is SpriteRenderer spriteRenderer && !spriteRenderer.IsNull())
                    spriteRenderer.enabled = _Activate;
            }
        }

        #endregion
    }
}
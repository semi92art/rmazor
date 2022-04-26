﻿using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public enum EProceedingStage
    {
        Inactive,
        Active,
        ActiveAndWorking
    }

    public abstract class ViewMazeItemBase : IViewMazeItem
    {
        #region nonpublic members
        
        protected abstract string ObjectName { get; }
        protected bool Initialized { get; set; }
        // ReSharper disable once InconsistentNaming
        protected bool m_ActivatedInSpawnPool;

        #endregion

        #region inject

        protected ViewSettings                  ViewSettings        { get; }
        protected IModelGame                    Model               { get; }
        protected IMazeCoordinateConverter      CoordinateConverter { get; }
        protected IContainersGetter             ContainersGetter    { get; }
        protected IViewGameTicker               GameTicker          { get; }
        protected IViewBetweenLevelTransitioner Transitioner        { get; }
        protected IManagersGetter               Managers            { get; }
        protected IColorProvider                ColorProvider       { get; }
        protected IViewInputCommandsProceeder   CommandsProceeder   { get; }


        protected ViewMazeItemBase(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder)
        {
            ViewSettings        = _ViewSettings;
            Model               = _Model;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            GameTicker          = _GameTicker;
            Transitioner        = _Transitioner;
            Managers            = _Managers;
            ColorProvider       = _ColorProvider;
            CommandsProceeder   = _CommandsProceeder;
        }

        #endregion
        
        #region api
        
        public abstract Component[]       Shapes { get; }
        public          ViewMazeItemProps Props  { get; set; }
        public abstract object            Clone();
        
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
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                    ProceedingStage = EProceedingStage.ActiveAndWorking;
                    break;
                case ELevelStage.Paused:
                case ELevelStage.ReadyToUnloadLevel:
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
            {
                GameTicker.Register(this);
                ColorProvider.ColorChanged += OnColorChanged;
                Object = new GameObject(ObjectName);
                InitShape();
            }
            Object.SetParent(ContainersGetter.GetContainer(ContainerNames.MazeItems).gameObject);
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
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
            Cor.Run(Cor.WaitWhile(
                () => !Initialized,
                () =>
                {
                    OnAppearStart(_Appear);
                    Transitioner.DoAppearTransition(
                        _Appear,
                        GetAppearSets(_Appear),
                        () => OnAppearFinish(_Appear));
                }));
        }

        #endregion
        
        #region nonpublic methods
        
        protected abstract void InitShape();
        protected abstract void UpdateShape();
        protected abstract void OnColorChanged(int _ColorId, Color _Color);

        protected virtual void OnAppearStart(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                ActivateShapes(true);
        }

        protected virtual Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {Shapes, () => ColorProvider.GetColor(ColorIds.MazeItem1)}
            };
        }

        protected virtual void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                ActivateShapes(false);
            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
        }

        private void ActivateShapes(bool _Activate)
        {
            foreach (var shape in Shapes)
            {
                switch (shape)
                {
                    case ShapeRenderer shapeRenderer when !shapeRenderer.IsNull():
                        shapeRenderer.enabled = _Activate; break;
                    case SpriteRenderer spriteRenderer when !spriteRenderer.IsNull():
                        spriteRenderer.enabled = _Activate; break;
                }
            }
        }

        protected int GetSortingOrder()
        {
            return SortingOrders.GetBlockSortingOrder(Props.Type);
        }

        #endregion
    }
}
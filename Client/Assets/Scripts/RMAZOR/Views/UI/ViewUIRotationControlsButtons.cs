﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class ViewUIRotationControlsButtons : IViewUIRotationControls
    {
        #region nonpublic members
        
        private readonly List<Component> m_Renderers             = new List<Component>();
        private readonly List<Component> m_RotatingButtonShapes  = new List<Component>();
        private readonly List<Component> m_RotatingButtonShapes2 = new List<Component>();
        
        private float           m_BottomOffset;
        private ButtonOnRaycast m_RotateClockwiseButton;
        private ButtonOnRaycast m_RotateCounterClockwiseButton;

        #endregion
        
        #region inject

        private IModelGame                    Model             { get; }
        private IColorProvider                ColorProvider     { get; }
        private ICameraProvider               CameraProvider    { get; }
        private IViewUITutorial               Tutorial          { get; }
        private IContainersGetter             ContainersGetter  { get; }
        private IManagersGetter               Managers          { get; }
        private IViewBetweenLevelTransitioner Transitioner      { get; }
        private IViewInputCommandsProceeder   CommandsProceeder { get; }
        private IViewInputTouchProceeder      TouchProceeder    { get; }
        private IViewGameTicker               GameTicker        { get; }

        public ViewUIRotationControlsButtons(
            IModelGame                    _Model,
            IColorProvider                _ColorProvider,
            ICameraProvider               _CameraProvider,
            IViewUITutorial               _Tutorial,
            IContainersGetter             _ContainersGetter,
            IManagersGetter               _Managers,
            IViewBetweenLevelTransitioner _Transitioner,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewInputTouchProceeder      _TouchProceeder,
            IViewGameTicker               _GameTicker)
        {
            Model             = _Model;
            ColorProvider     = _ColorProvider;
            CameraProvider    = _CameraProvider;
            Tutorial          = _Tutorial;
            ContainersGetter  = _ContainersGetter;
            Managers          = _Managers;
            Transitioner      = _Transitioner;
            CommandsProceeder = _CommandsProceeder;
            TouchProceeder    = _TouchProceeder;
            GameTicker        = _GameTicker;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_BottomOffset = _Offsets.z;
            InitRotateButtons();
            ColorProvider.ColorChanged += OnColorChanged;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            ShowRotatingButtons(_Args);
            if (_Args.Stage != ELevelStage.ReadyToStart 
                && _Args.Stage != ELevelStage.StartedOrContinued)
                return;
            bool enableRotation = Model.GetAllProceedInfos()
                .Any(_Info => RmazorUtils.GravityItemTypes.Contains(_Info.Type));
            var commands = RmazorUtils.RotateCommands;
            const string group = nameof(IViewInputTouchProceeder);
            if (enableRotation) 
                CommandsProceeder.UnlockCommands(commands, group);
            else
                CommandsProceeder.LockCommands(commands, group);
        }

        public List<Component> GetRenderers()
        {
            return new List<Component>();
        }

        public bool HasButtons                              => true;
        public void OnTutorialStarted(ETutorialType  _Type) { }
        public void OnTutorialFinished(ETutorialType _Type) { }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main) 
                return;
            foreach (var shapeComp in m_RotatingButtonShapes)
            {
                if (shapeComp is ShapeRenderer rend)
                    rend.Color = _Color;
            }
                
            foreach (var shapeComp in m_RotatingButtonShapes2)
            {
                if (shapeComp is ShapeRenderer rend)
                    rend.Color = _Color.SetA(0.2f);
            }
        }

        private void InitRotateButtons()
        {
            var screenBounds = GraphicUtils.GetVisibleBounds();
            const float horOffset = 3f;
            const float localScale = 1.5f;
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var goRcB = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "rotate_clockwise_button");
            var goRccB = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "rotate_counter_clockwise_button");
            float scale = 0.5f;
            float yPos = screenBounds.min.y + m_BottomOffset;
            var rcbDisc = goRcB.GetCompItem<Disc>("button");
            goRcB.transform.localScale = Vector3.one * (scale * localScale);
            goRcB.transform.SetPosXY(
                new Vector2(screenBounds.center.x, yPos) 
                + (Vector2.right + Vector2.up) * (scale * rcbDisc.Radius * localScale) 
                + Vector2.right * horOffset);
            goRccB.transform.localScale = Vector3.one * (scale * localScale);
            goRccB.transform.SetPosXY(
                new Vector2(screenBounds.center.x, yPos)
                + (Vector2.left + Vector2.up) * (scale * rcbDisc.Radius * localScale)
                + Vector2.left * horOffset);
            m_RotateClockwiseButton = goRcB.GetCompItem<ButtonOnRaycast>("button");
            m_RotateCounterClockwiseButton = goRccB.GetCompItem<ButtonOnRaycast>("button");
            m_RotateClockwiseButton.Init(
                () => CommandRotate(true), 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager,
                TouchProceeder);
            m_RotateCounterClockwiseButton.Init(
                () => CommandRotate(false), 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                TouchProceeder);
            m_RotatingButtonShapes.AddRange(new ShapeRenderer[]
            {
                goRcB.GetCompItem<Disc>("outer_disc"),
                goRcB.GetCompItem<Disc>("line"),
                goRcB.GetCompItem<Line>("arrow_part_1"), 
                goRcB.GetCompItem<Line>("arrow_part_2"), 
                goRccB.GetCompItem<Disc>("outer_disc"), 
                goRccB.GetCompItem<Disc>("line"),
                goRccB.GetCompItem<Line>("arrow_part_1"), 
                goRccB.GetCompItem<Line>("arrow_part_2")
            });
            
            m_RotatingButtonShapes2.Add(goRcB.GetCompItem<Disc>("inner_disc"));
            m_RotatingButtonShapes2.Add(goRccB.GetCompItem<Disc>("inner_disc"));
            
            goRcB.SetActive(false);
            goRccB.SetActive(false);
        }
        
        private void CommandRotate(bool _Clockwise)
        {
            // TODO
            // if (BigDialogViewer.IsShowing || BigDialogViewer.IsInTransition)
            //     return;

            switch (Model.LevelStaging.LevelStage)
            {
                case ELevelStage.Loaded:
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    return;
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                    bool raised = CommandsProceeder.RaiseCommand(
                        _Clockwise ? EInputCommand.RotateClockwise : EInputCommand.RotateCounterClockwise,
                        null);
                    if (!raised)
                        return;
                    Cor.Run(ButtonOnClickCoroutine(_Clockwise ? m_RotateClockwiseButton : m_RotateCounterClockwiseButton));
                    LockCommandsOnRotationStarted();
                    if (Model.LevelStaging.LevelStage == ELevelStage.ReadyToStart)
                        CommandsProceeder.RaiseCommand(EInputCommand.StartOrContinueLevel, null, true);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(Model.LevelStaging.LevelStage);
            }
        }
        
        private void LockCommandsOnRotationStarted()
        {
            CommandsProceeder.LockCommands(RmazorUtils.MoveAndRotateCommands, nameof(IViewInputTouchProceeder));
        }

        private void ShowRotatingButtons(LevelStageArgs _Args)
        {
            bool MazeContainsGravityItems()
            {
                return Model.GetAllProceedInfos()
                    .Any(_Info => RmazorUtils.GravityItemTypes.Contains(_Info.Type));
            }
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded when MazeContainsGravityItems():
                    ShowRotatingButtons(true, false);
                    break;
                case ELevelStage.Loaded:
                    ShowRotatingButtons(false, true);
                    break;
                case ELevelStage.ReadyToStart:
                    ShowRotatingButtons(MazeContainsGravityItems(), true);
                    break;
            }
        }
        
        private void ShowRotatingButtons(bool _Show, bool _Instantly)
        {
            if (_Instantly && !_Show || _Show)
            {
                m_RotateClockwiseButton.SetGoActive(_Show);
                m_RotateCounterClockwiseButton.SetGoActive(_Show);
                m_RotateClockwiseButton.enabled = _Show;
                m_RotateCounterClockwiseButton.enabled = _Show;
            }
            if (_Instantly)
                return;
            Transitioner.DoAppearTransition(_Show, 
                new Dictionary<IEnumerable<Component>, System.Func<Color>>
                {
                    {m_RotatingButtonShapes, () => ColorProvider.GetColor(ColorIds.UI)},
                    {m_RotatingButtonShapes2, () => ColorProvider.GetColor(ColorIds.UI).SetA(0.2f)}
                }, _Type: EAppearTransitionType.WithoutDelay);
        }
        
        private IEnumerator ButtonOnClickCoroutine(ButtonOnRaycast _Button)
        {
            var startScale = _Button.transform.localScale;
            const float minScale = 0.7f;
            yield return Cor.Lerp(
                GameTicker,
                0.1f,
                _OnProgress: _P => _Button.transform.localScale = startScale * _P,
                _ProgressFormula: _P =>
                {
                    if (_P < 0.5f)
                        return minScale + (1f - _P * 2f) * (1f - minScale);
                    return minScale + (_P * 2f - 1f) * (1f - minScale);
                });
        }

        #endregion
    }
}
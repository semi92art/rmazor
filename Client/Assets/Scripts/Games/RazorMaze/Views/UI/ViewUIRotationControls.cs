using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIRotationControls : 
        IOnLevelStageChanged,
        IInitViewUIItem,
        IViewUIGetRenderers { }
    
    public class ViewUIRotationControls : IViewUIRotationControls
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

        private IModelGame                  Model              { get; }
        private IColorProvider              ColorProvider      { get; }
        private IContainersGetter           ContainersGetter   { get; }
        private ICameraProvider             CameraProvider     { get; }
        private IManagersGetter             Managers           { get; }
        private IBigDialogViewer            BigDialogViewer    { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }
        private IViewGameTicker             GameTicker         { get; }
        private IViewAppearTransitioner     AppearTransitioner { get; }
        private IViewInputTouchProceeder    TouchProceeder     { get; }

        public ViewUIRotationControls(
            IModelGame _Model,
            IColorProvider _ColorProvider,
            IContainersGetter _ContainersGetter,
            ICameraProvider _CameraProvider,
            IManagersGetter _Managers,
            IBigDialogViewer _BigDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker _GameTicker,
            IViewAppearTransitioner _AppearTransitioner,
            IViewInputTouchProceeder _TouchProceeder)
        {
            Model = _Model;
            ColorProvider = _ColorProvider;
            ContainersGetter = _ContainersGetter;
            CameraProvider = _CameraProvider;
            Managers = _Managers;
            BigDialogViewer = _BigDialogViewer;
            CommandsProceeder = _CommandsProceeder;
            GameTicker = _GameTicker;
            AppearTransitioner = _AppearTransitioner;
            TouchProceeder = _TouchProceeder;
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
                .Any(_Info => RazorMazeUtils.GravityItemTypes().Contains(_Info.Type));
            if (enableRotation) 
                return;
            CommandsProceeder.LockCommands(RazorMazeUtils.GetRotateCommands());
        }

        public List<Component> GetRenderers()
        {
            return m_Renderers;
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.UI) 
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
            const float horOffset = 1f;
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var goRCb = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "rotate_clockwise_button");
            var goRCCb = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "rotate_counter_clockwise_button");
            float scale = 0.5f;
            float yPos = screenBounds.min.y + m_BottomOffset;
            var rcbDisc = goRCb.GetCompItem<Disc>("button");
            goRCb.transform.localScale = scale * Vector3.one;
            goRCb.transform.SetPosXY(
                new Vector2(screenBounds.center.x, yPos) 
                + (Vector2.right + Vector2.up) * scale * rcbDisc.Radius 
                + Vector2.right * horOffset);
            goRCCb.transform.localScale = scale * Vector3.one;
            goRCCb.transform.SetPosXY(
                new Vector2(screenBounds.center.x, yPos)
                + (Vector2.left + Vector2.up) * scale * rcbDisc.Radius
                + Vector2.left * horOffset);
            m_RotateClockwiseButton = goRCb.GetCompItem<ButtonOnRaycast>("button");
            m_RotateCounterClockwiseButton = goRCCb.GetCompItem<ButtonOnRaycast>("button");
            m_RotateClockwiseButton.Init(
                CommandRotateClockwise, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager,
                TouchProceeder);
            m_RotateCounterClockwiseButton.Init(
                CommandRotateCounterClockwise, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                TouchProceeder);
            m_RotatingButtonShapes.AddRange(new ShapeRenderer[]
            {
                goRCb.GetCompItem<Disc>("outer_disc"),
                goRCb.GetCompItem<Disc>("line"),
                goRCb.GetCompItem<Line>("arrow_part_1"), 
                goRCb.GetCompItem<Line>("arrow_part_2"), 
                goRCCb.GetCompItem<Disc>("outer_disc"), 
                goRCCb.GetCompItem<Disc>("line"),
                goRCCb.GetCompItem<Line>("arrow_part_1"), 
                goRCCb.GetCompItem<Line>("arrow_part_2")
            });
            
            m_RotatingButtonShapes2.Add(goRCb.GetCompItem<Disc>("inner_disc"));
            m_RotatingButtonShapes2.Add(goRCCb.GetCompItem<Disc>("inner_disc"));
            
            goRCb.SetActive(false);
            goRCCb.SetActive(false);
        }
        
        private void CommandRotateClockwise()
        {
            if (BigDialogViewer.IsShowing || BigDialogViewer.IsInTransition)
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.RotateClockwise, null);
            Coroutines.Run(ButtonOnClickCoroutine(m_RotateClockwiseButton));
        }
        
        private void CommandRotateCounterClockwise()
        {
            if (BigDialogViewer.IsShowing || BigDialogViewer.IsInTransition)
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.RotateCounterClockwise, null);
            Coroutines.Run(ButtonOnClickCoroutine(m_RotateCounterClockwiseButton));
        }
        
        private void ShowRotatingButtons(LevelStageArgs _Args)
        {
            bool MazeContainsGravityItems()
            {
                return Model.GetAllProceedInfos()
                    .Any(_Info => RazorMazeUtils.GravityItemTypes().Contains(_Info.Type));
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
            AppearTransitioner.DoAppearTransition(_Show, 
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {m_RotatingButtonShapes, () => ColorProvider.GetColor(ColorIds.UI)},
                    {m_RotatingButtonShapes2, () => ColorProvider.GetColor(ColorIds.UI).SetA(0.2f)}
                }, _Type: EAppearTransitionType.WithoutDelay);
        }
        
        private IEnumerator ButtonOnClickCoroutine(ButtonOnRaycast _Button)
        {
            var startScale = _Button.transform.localScale;
            const float minScale = 0.7f;
            yield return Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _P => _Button.transform.localScale = startScale * _P,
                GameTicker,
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
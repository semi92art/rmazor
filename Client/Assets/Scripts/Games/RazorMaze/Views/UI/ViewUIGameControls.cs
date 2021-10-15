using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIGameControls : IInit, IOnLevelStageChanged
    {
        
    }
    
    public class ViewUIGameControls : IViewUIGameControls
    {
        #region nonpublic members

        private ButtonOnRaycast m_RotateClockwiseButton;
        private ButtonOnRaycast m_RotateCounterClockwiseButton;
        private readonly List<ShapeRenderer> m_ButtonShapes = new List<ShapeRenderer>();

        #endregion
        
        #region inject
        
        private IModelGame Model { get; }
        private IContainersGetter ContainersGetter { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IViewInputConfigurator InputConfigurator { get; }
        private IViewAppearTransitioner AppearTransitioner { get; }
        private IGameTicker GameTicker { get; }

        public ViewUIGameControls(
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter,
            IViewInputConfigurator _InputConfigurator,
            IViewAppearTransitioner _AppearTransitioner,
            IGameTicker _GameTicker)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            InputConfigurator = _InputConfigurator;
            AppearTransitioner = _AppearTransitioner;
            GameTicker = _GameTicker;
        }

        #endregion
        
        #region api

        public event UnityAction Initialized;
        public void Init()
        {
            InitRotateButtons();
            Initialized?.Invoke();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            System.Func<bool> mazeContainsGravityItems = () => Model.GetAllProceedInfos()
                .Any(_Info => _Info.Type == EMazeItemType.GravityBlock
                              || _Info.Type == EMazeItemType.GravityTrap);
            if (_Args.Stage == ELevelStage.Loaded)
            {
                if (mazeContainsGravityItems())
                {
                    EnableRotatingButtons(true);
                    ShowRotatingButtons(true, false);
                }
                else
                {
                    EnableRotatingButtons(false);
                    ShowRotatingButtons(false, true);
                }
            }
            else if (_Args.Stage == ELevelStage.ReadyToStartOrContinue)
            {
                EnableRotatingButtons(mazeContainsGravityItems());
            }
            else if (_Args.Stage == ELevelStage.Finished)
            {
                EnableRotatingButtons(false);
                ShowRotatingButtons(false, false);
            }
        }

        #endregion

        #region nonpublic methds

        private void InitRotateButtons()
        {
            const float bottomOffset = 1f;
            const float horOffset = 1f;
            var cont = ContainersGetter.GetContainer(ContainerNames.GameMenu);
            var goRCb = PrefabUtilsEx.InitPrefab(
                cont, "ui_buttons", "rotate_clockwise_button");
            var goRCCb = PrefabUtilsEx.InitPrefab(
                cont, "ui_buttons", "rotate_counter_clockwise_button");

            // float scale = CoordinateConverter.Scale; // FIXME
            float scale = 1f;
            var bounds = GameUtils.GetVisibleBounds();
            var rcbDisc = goRCb.GetCompItem<Disc>("button");
            goRCb.transform.localScale = scale * Vector3.one;
            goRCb.transform.SetPosXY(
                new Vector2(bounds.max.x, bounds.min.y) 
                + scale * rcbDisc.Radius * (Vector2.left + Vector2.up)
                + Vector2.left * horOffset + Vector2.up * bottomOffset);
            goRCCb.transform.localScale = scale * Vector3.one;
            goRCCb.transform.SetPosXY(
                (Vector2)bounds.min
                + scale * rcbDisc.Radius * (Vector2.right + Vector2.up)
                + Vector2.right * horOffset + Vector2.up * bottomOffset);

            m_RotateClockwiseButton = goRCb.GetCompItem<ButtonOnRaycast>("button");
            m_RotateCounterClockwiseButton = goRCCb.GetCompItem<ButtonOnRaycast>("button");
            
            m_RotateClockwiseButton.OnClickEvent.AddListener(CommandRotateClockwise);
            m_RotateCounterClockwiseButton.OnClickEvent.AddListener(CommandRotateCounterClockwise);
            
            m_ButtonShapes.AddRange(new ShapeRenderer[]
            {
                goRCb.GetCompItem<Disc>("button"), 
                goRCb.GetCompItem<Disc>("line"),
                goRCb.GetCompItem<Line>("arrow_part_1"), 
                goRCb.GetCompItem<Line>("arrow_part_2"), 
                goRCCb.GetCompItem<Disc>("button"), 
                goRCCb.GetCompItem<Disc>("line"),
                goRCCb.GetCompItem<Line>("arrow_part_1"), 
                goRCCb.GetCompItem<Line>("arrow_part_2")
            });
            
            m_RotateClockwiseButton.SetGoActive(false);
            m_RotateCounterClockwiseButton.SetGoActive(false);
        }

        private void ShowRotatingButtons(bool _Show, bool _Instantly)
        {
            if (_Instantly && !_Show || _Show)
            {
                m_RotateClockwiseButton.SetGoActive(_Show);
                m_RotateCounterClockwiseButton.SetGoActive(_Show);
                
            }

            if (_Instantly)
                return;
            
            AppearTransitioner.DoAppearTransitionSimple(_Show, GameTicker, 
                new Dictionary<object[], System.Func<Color>>
                {
                    {m_ButtonShapes.Cast<object>().ToArray(), () => DrawingUtils.ColorLines}
                }, _Type: EAppearTransitionType.WithoutDelay);
        }

        private void EnableRotatingButtons(bool _Enable)
        {
            m_RotateClockwiseButton.enabled = _Enable;
            m_RotateCounterClockwiseButton.enabled = _Enable;
        }

        private void CommandRotateClockwise()
        {
            InputConfigurator.RaiseCommand(InputCommands.RotateClockwise, null);
        }
        
        private void CommandRotateCounterClockwise()
        {
            InputConfigurator.RaiseCommand(InputCommands.RotateCounterClockwise, null);
        }

        #endregion


    }
}
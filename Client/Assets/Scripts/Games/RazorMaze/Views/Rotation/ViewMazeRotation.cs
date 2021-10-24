using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Rotation
{
    public class ViewMazeRotation : ViewMazeRotationBase, IUpdateTick
    {
        #region nonpublic members

        private Rigidbody2D m_Rb;
        private Disc m_Disc;
        private bool m_EnableRotation;

        #endregion
        
        #region inject
        
        private ViewSettings ViewSettings { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private IGameTicker GameTicker { get; }
        private IModelGame Model { get; }
        private IViewCharacter Character { get; }
        private IViewAppearTransitioner Transitioner { get; }
        private IViewInputConfigurator InputConfigurator { get; }

        public ViewMazeRotation(
            ViewSettings _ViewSettings,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter, 
            IGameTicker _GameTicker,
            IModelGame _Model,
            IViewCharacter _Character,
            IViewAppearTransitioner _Transitioner,
            IViewInputConfigurator _InputConfigurator)
        {
            ViewSettings = _ViewSettings;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Model = _Model;
            Character = _Character;
            Transitioner = _Transitioner;
            InputConfigurator = _InputConfigurator;
            _GameTicker.Register(this);
        }
        
        #endregion

        #region api
        
        public override event UnityAction<float> RotationContinued;
        public override event MazeOrientationHandler RotationFinished;

        public override void Init()
        {
            var cont = ContainersGetter.GetContainer(ContainerNames.Maze);
            
            m_Rb = cont.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            m_Rb.constraints = RigidbodyConstraints2D.FreezePosition;
            
            m_Disc = cont.gameObject.AddComponentOnNewChild<Disc>("Rotation Disc", out _);
            m_Disc.Dashed = true;
            m_Disc.Type = DiscType.Ring;
            m_Disc.DashType = DashType.Rounded;
            m_Disc.DashSize = 50f;
        }

        public override void OnRotationStarted(MazeRotationEventArgs _Args)
        {
            if (_Args.Instantly)
            {
                GetRotationParams(_Args,
                    out _,
                    out float endAngle,
                    out _,
                    out _);
                m_Rb.SetRotation(endAngle);
            }
            else
                Coroutines.Run(RotationCoroutine(_Args));
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            bool? appear = null;
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    m_Disc.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
                    m_Disc.Radius = CalculateRotationDiscRadius();
                        m_EnableRotation = Model.GetAllProceedInfos().Any(_Info =>
                        _Info.Type == EMazeItemType.GravityBlock
                        || _Info.Type == EMazeItemType.GravityTrap
                        || _Info.Type == EMazeItemType.GravityBlockFree);

                    m_Disc.enabled = m_EnableRotation;
                    if (m_EnableRotation)
                        appear = true;
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                    if (_Args.PreviousStage != ELevelStage.Loaded)
                        return;
                    if (!m_EnableRotation)
                    {
                        InputConfigurator.LockCommand(InputCommands.RotateClockwise);
                        InputConfigurator.LockCommand(InputCommands.RotateCounterClockwise);
                    }
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    appear = false;
                    break;
            }

            if (!appear.HasValue)
                return;
            Transitioner.DoAppearTransitionSimple(
                appear.Value,
                GameTicker,
                new Dictionary<object[], Func<Color>>
                {
                    {new object[] { m_Disc }, () => DrawingUtils.ColorLines.SetA(0.5f)}
                },
                _Type: EAppearTransitionType.WithoutDelay);
        }
        
        public void UpdateTick()
        {
            m_Disc.DashOffset += Time.deltaTime * -0.5f;
        }

        #endregion

        #region nonpublic methods

        private float CalculateRotationDiscRadius()
        {
            var infos = Model.GetAllProceedInfos();
            var pathPoints = Model.PathItemsProceeder.PathProceeds.Keys.ToList();
            var points = infos.Select(_Info => _Info.CurrentPosition).Concat(pathPoints);
            var maxDistance = points.Max(_P => (_P - Model.Data.Info.Size).ToVector2().magnitude);
            return CoordinateConverter.Scale * maxDistance * 0.5f;
        }
        
        private IEnumerator RotationCoroutine(MazeRotationEventArgs _Args)
        {
            GetRotationParams(_Args,
                out float startAngle,
                out float endAngle,
                out float rotationAngleDistance,
                out float realSkidAngle);
            
            yield return Coroutines.Lerp(
                startAngle, 
                endAngle + realSkidAngle, 
                rotationAngleDistance / (90f * ViewSettings.MazeRotationSpeed),
                _Angle => m_Rb.SetRotation(_Angle),
                GameTicker, 
                (_, __) =>
                {
                    RotationFinished?.Invoke(_Args);
                    Coroutines.Run(Coroutines.Lerp(
                        endAngle + realSkidAngle,
                        endAngle,
                        1 / (ViewSettings.MazeRotationSpeed * 2f),
                        _Angle => m_Rb.SetRotation(_Angle),
                        GameTicker,
                        (___, ____) => Character.OnRotationAfterFinished(_Args)));
                });
        }

        private void GetRotationParams(MazeRotationEventArgs _Args, 
            out float _StartAngle,
            out float _EndAngle,
            out float _RotationAngleDistance,
            out float _RealSkidAngle)
        {
            _StartAngle = GetAngleByOrientation(_Args.CurrentOrientation);
            float dirCoeff = _Args.Direction == MazeRotateDirection.Clockwise ? -1 : 1;
            _RotationAngleDistance = GetRotationAngleDistance(_Args);
            _EndAngle = _StartAngle + _RotationAngleDistance * dirCoeff;
            _RealSkidAngle = 3f * dirCoeff;
        }

        private float GetRotationAngleDistance(MazeRotationEventArgs _Args)
        {
            var dir = _Args.Direction;
            var from = (int)_Args.CurrentOrientation;
            var to = (int)_Args.NextOrientation;

            if (from == to)
                return 0f;
            if (dir == MazeRotateDirection.Clockwise && to < from)
                return 90f * (4 - from + to);
            if (dir == MazeRotateDirection.CounterClockwise && to > from)
                return 90f * (4 - to + from);
            return 90f * Mathf.Abs(to - from);
        }

        #endregion
    }
}
using System.Collections;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Rotation
{
    public class ViewMazeRotation : ViewMazeRotationBase
    {
        #region nonpublic members

        private Rigidbody2D m_Rb;

        #endregion
        
        #region inject
        
        private ViewSettings ViewSettings { get; }
        private IContainersGetter ContainersGetter { get; }
        private IGameTicker GameTicker { get; }
        private IModelGame Model { get; }
        private IViewCharacter Character { get; }

        public ViewMazeRotation(
            ViewSettings _ViewSettings,
            IContainersGetter _ContainersGetter, 
            IGameTicker _GameTicker,
            IModelGame _Model,
            IViewCharacter _Character)
        {
            ViewSettings = _ViewSettings;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Model = _Model;
            Character = _Character;
        }
        
        #endregion

        #region api
        
        public override event UnityAction<float> RotationContinued;
        public override event MazeOrientationHandler RotationFinished;

        public override void Init()
        {
            m_Rb = ContainersGetter.MazeContainer.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
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

        }

        #endregion

        #region nonpublic methods

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
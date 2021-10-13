using System.Collections;
using Games.RazorMaze.Models;
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

        public ViewMazeRotation(
            ViewSettings _ViewSettings,
            IContainersGetter _ContainersGetter, 
            IGameTicker _GameTicker,
            IModelGame _Model)
        {
            ViewSettings = _ViewSettings;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Model = _Model;
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
            Coroutines.Run(RotationCoroutine(_Args));
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {

        }

        #endregion

        #region nonpublic methods

        private IEnumerator RotationCoroutine(MazeRotationEventArgs _Args)
        {
            const float skidAngle = 3f;
            float startAngle = GetAngleByOrientation(_Args.CurrentOrientation);
            float dirCoeff = _Args.Direction == MazeRotateDirection.Clockwise ? -1 : 1;
            float rotAngDist = GetRotationAngleDistance(_Args);
            float realSkidAngle = skidAngle * dirCoeff;
            float endAngle = startAngle + rotAngDist * dirCoeff;
            
            yield return Coroutines.Lerp(
                startAngle, 
                endAngle + realSkidAngle, 
                rotAngDist / (90f * ViewSettings.MazeRotationSpeed),
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
                        GameTicker));
                });
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
using System.Collections;
using Common.Constants;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Characters;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Rotation
{
    public class ViewMazeRotation : ViewMazeRotationBase
    {
        #region nonpublic members

        private Rigidbody2D m_Rb;

        #endregion
        
        #region inject
        
        private ViewSettings                ViewSettings        { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IViewGameTicker             GameTicker          { get; }
        private IViewCharacter              Character           { get; }

        private ViewMazeRotation(
            ViewSettings                _ViewSettings,
            IContainersGetter           _ContainersGetter, 
            IViewGameTicker             _GameTicker,
            IViewCharacter              _Character)
        {
            ViewSettings      = _ViewSettings;
            ContainersGetter  = _ContainersGetter;
            GameTicker        = _GameTicker;
            Character         = _Character;
        }
        
        #endregion

        #region api

        public sealed override event UnityAction<float>     RotationContinued;
        public sealed override event MazeOrientationHandler RotationFinished;

        public override void Init()
        {
            var cont = ContainersGetter.GetContainer(ContainerNamesMazor.MazeHolder);
            m_Rb = cont.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            m_Rb.constraints = RigidbodyConstraints2D.FreezeAll;
            base.Init();
        }

        public override void OnMazeRotationStarted(MazeRotationEventArgs _Args)
        {
            if (_Args.Instantly)
                m_Rb.transform.eulerAngles = Vector3.zero;
            else
                Cor.Run(RotationCoroutine(_Args));
            m_Rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public override void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            m_Rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            // do nothing
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
            
            yield return Cor.Lerp(
                GameTicker, 
                rotationAngleDistance / (90f * ViewSettings.mazeRotationSpeed),
                startAngle,
                endAngle + realSkidAngle,
                _Angle => m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle),
                () =>
                {
                    RotationFinished?.Invoke(_Args);
                    Cor.Run(Cor.Lerp(
                        GameTicker,
                        1 / (ViewSettings.mazeRotationSpeed * 2f),
                        endAngle + realSkidAngle,
                        endAngle,
                        _Angle => m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle),
                        () => Character.OnRotationFinished(_Args)));
                });
        }

        private void GetRotationParams(MazeRotationEventArgs _Args, 
            out float _StartAngle,
            out float _EndAngle,
            out float _RotationAngleDistance,
            out float _RealSkidAngle)
        {
            _StartAngle = GetAngleByOrientation(_Args.CurrentOrientation);
            float dirCoeff = _Args.Direction == EMazeRotateDirection.Clockwise ? -1 : 1;
            _RotationAngleDistance = GetRotationAngleDistance(_Args);
            _EndAngle = _StartAngle + _RotationAngleDistance * dirCoeff;
            _RealSkidAngle = 3f * dirCoeff;
        }

        private static float GetRotationAngleDistance(MazeRotationEventArgs _Args)
        {
            var dir = _Args.Direction;
            var from = (int)_Args.CurrentOrientation;
            var to = (int)_Args.NextOrientation;

            if (from == to)
                return 0f;
            if (dir == EMazeRotateDirection.Clockwise && to < from)
                return 90f * (4 - from + to);
            if (dir == EMazeRotateDirection.CounterClockwise && to > from)
                return 90f * (4 - to + from);
            return 90f * Mathf.Abs(to - from);
        }

        #endregion
    }
}
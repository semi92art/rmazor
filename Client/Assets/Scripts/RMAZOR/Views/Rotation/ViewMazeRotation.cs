using System.Collections;
using Common.Constants;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Characters;
using RMAZOR.Views.InputConfigurators;
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
        private IModelGame                  Model               { get; }
        private IViewCharacter              Character           { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }

        public ViewMazeRotation(
            ViewSettings                _ViewSettings,
            IContainersGetter           _ContainersGetter, 
            IViewGameTicker             _GameTicker,
            IModelGame                  _Model,
            IViewCharacter              _Character,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            ViewSettings      = _ViewSettings;
            ContainersGetter  = _ContainersGetter;
            GameTicker        = _GameTicker;
            Model             = _Model;
            Character         = _Character;
            CommandsProceeder = _CommandsProceeder;
        }
        
        #endregion

        #region api
        
        public sealed override event  UnityAction<float>     RotationContinued;
        public sealed override event MazeOrientationHandler RotationFinished;

        public override void Init()
        {
            var cont = ContainersGetter.GetContainer(ContainerNames.MazeHolder);
            m_Rb = cont.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            m_Rb.constraints = RigidbodyConstraints2D.FreezePosition;
            base.Init();
        }

        public override void OnRotationStarted(MazeRotationEventArgs _Args)
        {
            if (_Args.Instantly)
                m_Rb.transform.eulerAngles = Vector3.zero;
            else
                Cor.Run(RotationCoroutine(_Args));
        }

        public override void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            // do nothing
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
                startAngle,
                endAngle + realSkidAngle,
                rotationAngleDistance / (90f * ViewSettings.MazeRotationSpeed),
                _Angle => m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle),
                GameTicker, 
                (_, __) =>
                {
                    RotationFinished?.Invoke(_Args);
                    Cor.Run(Cor.Lerp(
                        endAngle + realSkidAngle,
                        endAngle,
                        1 / (ViewSettings.MazeRotationSpeed * 2f),
                        _Angle => m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle),
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
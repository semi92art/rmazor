using System.Collections;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine;
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

        public ViewMazeRotation(
            ViewSettings _ViewSettings,
            IContainersGetter _ContainersGetter, 
            IGameTicker _GameTicker)
        {
            ViewSettings = _ViewSettings;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
        }
        
        #endregion

        #region api

        public override event FloatHandler RotationContinued;
        public override event MazeOrientationHandler RotationFinished;

        public override void Init()
        {
            m_Rb = ContainersGetter.MazeContainer.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            base.Init();
        }

        public override void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            Coroutines.Run(RotationCoroutine(_Direction, _Orientation));
        }

        #endregion

        #region nonpublic methods

        private IEnumerator RotationCoroutine(
            MazeRotateDirection _Direction,
            MazeOrientation _Orientation)
        {
            float startAngle = GetAngleByOrientation(_Orientation);
            float dirCoeff = _Direction == MazeRotateDirection.Clockwise ? -1 : 1;
            const float skidAngle = 5f;
            float realSkidAngle = skidAngle * dirCoeff;
            float endAngle = startAngle + 90f * dirCoeff;
            
            yield return Coroutines.Lerp(
                startAngle, 
                endAngle + realSkidAngle, 
                1 / ViewSettings.MazeRotationSpeed,
                _Angle => m_Rb.SetRotation(_Angle),
                GameTicker, 
                (_, __) =>
                {
                    var nextOrientation = GetNextOrientation(_Direction, _Orientation);
                    RotationFinished?.Invoke(_Direction, nextOrientation);
                    Coroutines.Run(Coroutines.Lerp(
                        endAngle + realSkidAngle,
                        endAngle,
                        1 / ViewSettings.MazeRotationSpeed,
                        _Angle => m_Rb.SetRotation(_Angle),
                        GameTicker));
                });
        }

        #endregion
    }
}
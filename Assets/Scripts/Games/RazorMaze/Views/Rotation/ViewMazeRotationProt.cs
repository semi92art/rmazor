using Exceptions;
using Games.RazorMaze.Models;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Rotation
{
    public class ViewMazeRotationProt : IViewMazeRotation
    {
        private Rigidbody2D m_Rb;
        private float m_StartAngle;
        private MazeRotateDirection m_Direction;
        private MazeOrientation m_Orientation;
        
        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        
        public ViewMazeRotationProt(IContainersGetter _ContainersGetter)
        {
            ContainersGetter = _ContainersGetter;
        }
        
        #endregion
        
        public void Init()
        {
            m_Rb = ContainersGetter.MazeContainer.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
        }
        
        public void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            m_Direction = _Direction;
            m_Orientation = _Orientation;
            var prevOrientantion = GetPreviousOrientation(m_Direction, m_Orientation);
            float angle = GetAngleByOrientation(prevOrientantion);
            m_Rb.SetRotation(angle);
            m_StartAngle = ContainersGetter.MazeContainer.localEulerAngles.z;
        }

        public void Rotate(float _Progress)
        {
            float dirCorff = m_Direction == MazeRotateDirection.Clockwise ? -1 : 1;
            float currAngle = m_StartAngle + _Progress * 90f * dirCorff;
            m_Rb.SetRotation(currAngle);
        }

        public void FinishRotation()
        {
            float angle = GetAngleByOrientation(m_Orientation);
            m_Rb.SetRotation(angle);
        }
        
        private static MazeOrientation GetPreviousOrientation(
            MazeRotateDirection _Direction,
            MazeOrientation _Orientation)
        {
            int orient = (int) _Orientation;
            switch (_Direction)
            {
                case MazeRotateDirection.Clockwise:
                    orient = MathUtils.ClampInverse(orient - 1, 0, 3); break;
                case MazeRotateDirection.CounterClockwise:
                    orient = MathUtils.ClampInverse(orient + 1, 0, 3); break;
                default: throw new SwitchCaseNotImplementedException(_Direction);
            }
            return (MazeOrientation) orient;
        }
        
        private static float GetAngleByOrientation(MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North: return 0;
                case MazeOrientation.East:  return 270;
                case MazeOrientation.South: return 180;
                case MazeOrientation.West:  return 90;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }
    }
}
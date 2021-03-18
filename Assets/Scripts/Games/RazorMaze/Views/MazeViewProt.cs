using System.Collections.Generic;
using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public class MazeViewProt : IMazeView
    {
        private IMazeModel Model { get; }
        private ICoordinateConverter Scaler { get; }

        private Transform m_Container;
        private List<MazeProtItem> m_MazeItems;
        private Rigidbody2D m_Rb;
        private MazeRotateDirection m_Direction;
        private MazeOrientation m_Orientation;
        private float m_StartAngle;
        
        public MazeViewProt(IMazeModel _Model, ICoordinateConverter _Scaler)
        {
            Model = _Model;
            Scaler = _Scaler;
        }
        
        public void Init()
        {
            m_Container = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
            m_Rb = m_Container.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            m_MazeItems = RazorMazePrototypingUtils.CreateMazeItems(Model.Info, m_Container);
        }

        public void SetLevel(int _Level) { }
        
        public void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            m_Direction = _Direction;
            m_Orientation = _Orientation;
            var prevOrientantion = GetPreviousOrientation(m_Direction, m_Orientation);
            float angle = GetAngleByOrientation(prevOrientantion);
            m_Rb.SetRotation(angle);
            m_StartAngle = m_Container.localEulerAngles.z;
        }

        public void Rotate(float _Progress)
        {
            float dirCorff = m_Direction == MazeRotateDirection.Clockwise ? -1 : 1;
            float currAngle = m_StartAngle + RotateCoefficient(_Progress) * 90f * dirCorff;
            m_Rb.SetRotation(currAngle);
        }

        public void FinishRotation()
        {
            float angle = GetAngleByOrientation(m_Orientation);
            m_Rb.SetRotation(angle);
        }

        private static float RotateCoefficient(float _Progress) => Mathf.Pow(_Progress, 2);

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
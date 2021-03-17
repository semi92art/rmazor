using Exceptions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public class MazeViewProt : IMazeView
    {
        private MazeProtItems Maze { get; set; }
        private IMazeModel Model { get; set; }

        private MazeRotateDirection m_Direction;
        private MazeOrientation m_Orientation;
        private float m_StartAngle;

        public void Init(IMazeModel _Model)
        {
            Model = _Model;
            Maze = MazeProtItems.Create(
                _Model.Info, 
                CommonUtils.FindOrCreateGameObject("Maze", out _).transform, 
                true);
        }

        public void SetLevel(int _Level) { }
        
        public void StartRotation(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            m_Direction = _Direction;
            m_Orientation = _Orientation;
            var rb = Maze.Container.GetComponent<Rigidbody2D>();
            var prevOrientantion = GetPreviousOrientation(m_Direction, m_Orientation);
            float angle = GetAngleByOrientation(prevOrientantion);
            rb.SetRotation(angle);
            m_StartAngle = Maze.Container.localEulerAngles.z;
        }

        public void Rotate(float _Progress)
        {
            var rb = Maze.Container.GetComponent<Rigidbody2D>();
            float dirCorff = m_Direction == MazeRotateDirection.Clockwise ? -1 : 1;
            float currAngle = m_StartAngle + RotateCoefficient(_Progress) * 90f * dirCorff;
            rb.SetRotation(currAngle);
        }

        public void FinishRotation()
        {
            var rb = Maze.Container.GetComponent<Rigidbody2D>();
            float angle = GetAngleByOrientation(m_Orientation);
            rb.SetRotation(angle);
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
using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public class MazeViewProt : IMazeView
    {
        private IMazeModel Model { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        
        private List<MazeProtItem> m_MazeItems;
        private readonly List<Tuple<Obstacle, Vector2, Vector2>> m_MovingObstacles = new List<Tuple<Obstacle, Vector2, Vector2>>();
        
        private Rigidbody2D m_Rb;
        private MazeRotateDirection m_Direction;
        private MazeOrientation m_Orientation;
        private float m_StartAngle;
        
        public MazeViewProt(
            IMazeModel _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter)
        {
            Model = _Model;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
        }
        
        public void Init()
        {
            m_Rb = ContainersGetter.MazeContainer.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            m_MazeItems = RazorMazePrototypingUtils.CreateMazeItems(Model.Info, ContainersGetter.MazeItemsContainer);
            CoordinateConverter.Init(Model.Info.Width);
            ContainersGetter.MazeItemsContainer.SetLocalPosXY(Vector2.zero);
            ContainersGetter.MazeItemsContainer.PlusLocalPosY(CoordinateConverter.GetScale() * 0.5f);
            DrawWallBlockMovingPaths();
        }

        public void SetLevel(int _Level) { }
        
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
            float currAngle = m_StartAngle + RotateCoefficient(_Progress) * 90f * dirCorff;
            m_Rb.SetRotation(currAngle);
        }

        public void FinishRotation()
        {
            float angle = GetAngleByOrientation(m_Orientation);
            m_Rb.SetRotation(angle);
        }

        public void OnObstacleMoveStarted(Obstacle _Obstacle, V2Int _From, V2Int _To)
        {
            var obstacleItem = m_MovingObstacles
                .SingleOrDefault(_Item => _Item.Item1 == _Obstacle);
            if (obstacleItem != null)
                m_MovingObstacles.Remove(obstacleItem);
            m_MovingObstacles.Add(new Tuple<Obstacle, Vector2, Vector2>(
                _Obstacle,
                CoordinateConverter.ToLocalMazeItemPosition(_From),
                CoordinateConverter.ToLocalMazeItemPosition(_To)));
        }

        public void OnObstacleMove(Obstacle _Obstacle, float _Progress)
        {
            var obstacle = m_MovingObstacles
                .SingleOrDefault(_Tuple => _Tuple.Item1 == _Obstacle);
            if (obstacle == null)
                return;
            var mazeItem = m_MazeItems.Single(_Item => _Item.Equal(_Obstacle));
            var pos = Vector2.Lerp(obstacle.Item2, obstacle.Item3, _Progress);
            mazeItem.SetLocalPosition(pos);
        }
        
        public void OnObstacleMoveFinished(Obstacle _Obstacle)
        {
            var obstacleItem = m_MovingObstacles
                .SingleOrDefault(_Item => _Item.Item1 == _Obstacle);
            if (obstacleItem == null)
                return;
            m_MovingObstacles.Remove(obstacleItem);
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

        private void DrawWallBlockMovingPaths()
        {
            var obstacles = Model.Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.ObstacleMoving);
            foreach (var obs in obstacles)
            {
                var go = new GameObject("Line");
                go.SetParent(ContainersGetter.MazeContainer);
                go.transform.SetLocalPosXY(Vector2.zero);
                var line = go.AddComponent<Polyline>();
                line.Thickness = 0.3f;
                line.Color = Color.black;
                line.SetPoints(obs.Path.Select(_P => CoordinateConverter.ToLocalCharacterPosition(_P)).ToList());
                line.SortingOrder = 4;
                foreach (var pathItem in obs.Path)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeContainer);
                    var disc = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(CoordinateConverter.ToLocalCharacterPosition(pathItem));
                    disc.Color = Color.black;
                    disc.Radius = 0.5f;
                    disc.Type = DiscType.Disc;
                }
            }
        }
    }
}
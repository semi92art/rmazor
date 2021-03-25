using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using ModestTree;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public class MazeViewProt : IMazeView
    {
        private IMazeModel Model { get; }
        public IMazeTransformer Transformer { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        
        private List<MazeProtItem> m_MazeItems;
        private Rigidbody2D m_Rb;
        private MazeRotateDirection m_Direction;
        private MazeOrientation m_Orientation;
        private float m_StartAngle;
        private readonly List<Tuple<MazeItem, Vector2, Vector2>> m_MazeItemsMoving = new List<Tuple<MazeItem, Vector2, Vector2>>();
        private Dictionary<MazeItem, List<Tuple<V2Int, Disc>>> m_BusyPositions = new Dictionary<MazeItem, List<Tuple<V2Int, Disc>>>();
        
        public MazeViewProt(
            IMazeModel _Model,
            IMazeTransformer _Transformer,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter)
        {
            Model = _Model;
            Transformer = _Transformer;
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

        public void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            var mazeItem = m_MazeItemsMoving
                .SingleOrDefault(_Item => _Item.Item1 == _Args.Item);
            if (mazeItem != null)
                m_MazeItemsMoving.Remove(mazeItem);
            m_MazeItemsMoving.Add(new Tuple<MazeItem, Vector2, Vector2>(
                _Args.Item,
                CoordinateConverter.ToLocalMazeItemPosition(_Args.From),
                CoordinateConverter.ToLocalMazeItemPosition(_Args.To)));
        }

        public void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.BlockMovingGravity)
                MarkMazeItemBusyPositions(_Args.Item, Transformer.GravityProceeds[_Args.Item].BusyPositions);
            var item = m_MazeItemsMoving
                .SingleOrDefault(_Tuple => _Tuple.Item1 == _Args.Item);
            if (item == null)
                return;
            var mazeItem = m_MazeItems.Single(_Item => _Item.Equal(_Args.Item));
            var pos = Vector2.Lerp(item.Item2, item.Item3, _Args.Progress);
            mazeItem.SetLocalPosition(pos);
        }
        
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.BlockMovingGravity)
                MarkMazeItemBusyPositions(_Args.Item, null);
            var item = m_MazeItemsMoving
                .SingleOrDefault(_Item => _Item.Item1 == _Args.Item);
            if (item == null)
                return;
            var mazeItem = m_MazeItems.Single(_Item => _Item.Equal(_Args.Item));
            mazeItem.SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            m_MazeItemsMoving.Remove(item);
        }

        private static float RotateCoefficient(float _Progress) => _Progress;// Mathf.Pow(_Progress, 2);

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
            var items = Model.Info.MazeItems
                .Where(_O => _O.Type == EMazeItemType.BlockMovingGravity
                || _O.Type == EMazeItemType.TrapMovingGravity
                || _O.Type == EMazeItemType.TrapMoving);
            foreach (var obs in items)
            {
                var points = obs.Path.Select(_P => CoordinateConverter.ToLocalCharacterPosition(_P)).ToList();

                var go = new GameObject("Line");
                go.SetParent(ContainersGetter.MazeContainer);
                go.transform.SetLocalPosXY(Vector2.zero);
                var line = go.AddComponent<Polyline>();
                line.Thickness = 0.3f;
                line.Color = Color.black;
                line.SetPoints(points);
                line.Closed = false;

                foreach (var point in points)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeContainer);
                    var disc = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(point);
                    disc.Color = Color.black;
                    disc.Radius = 0.5f;
                    disc.Type = DiscType.Disc;
                }
            }
        }

        private void MarkMazeItemBusyPositions(MazeItem _Item, IEnumerable<V2Int> _Positions)
        {
            if (!m_BusyPositions.ContainsKey(_Item))
                m_BusyPositions.Add(_Item, new List<Tuple<V2Int, Disc>>());

            var busyPoss = m_BusyPositions[_Item];

            foreach (var disc in busyPoss.Select(_Tup => _Tup.Item2))
            {
                if (!disc.IsNull())
                    disc.DestroySafe();
            }
            busyPoss.Clear();

            if (_Positions == null)
                return;
            
            foreach (var pos in _Positions)
            {
                var go = new GameObject("Busy Pos");
                go.SetParent(ContainersGetter.MazeContainer);
                go.transform.localPosition = CoordinateConverter.ToLocalCharacterPosition(pos);
                var disc = go.AddComponent<Disc>();
                disc.Color = Color.red;
                disc.Radius = 0.3f;
                disc.SortingOrder = 20;
                var tup = new Tuple<V2Int, Disc>(pos, disc);
                busyPoss.Add(tup);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using Shapes;
using TimeProviders;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze
{
    public static class RazorMazeUtils
    {
        #region api

        public const float Epsilon = 1e-5f;
        
        public static List<MazeItem> GetBlockMazeItems(IEnumerable<MazeItem> _Items)
        {
            return _Items.Where(_Item =>
                _Item.Type == EMazeItemType.Block
                || _Item.Type == EMazeItemType.Turret
                || _Item.Type == EMazeItemType.TrapReact
                || _Item.Type == EMazeItemType.TrapIncreasing
                || _Item.Type == EMazeItemType.Attenuator
                || _Item.Type == EMazeItemType.GravityBlock
                || _Item.Type == EMazeItemType.ShredingerBlock).ToList();
        }

        public static bool IsValidPositionForMove(MazeInfo _Info, V2Int _Position)
        {
            bool isOnNode = _Info.Path.Any(_N => _N == _Position);
            var blockItems = GetBlockMazeItems(_Info.MazeItems);
            bool isOnBlockItem = blockItems.Any(_N => _N.Position == _Position);
            return isOnNode && !isOnBlockItem;
        }
        
        public static V2Int GetDropDirection(MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North: return V2Int.down;
                case MazeOrientation.South: return V2Int.up;
                case MazeOrientation.East:  return V2Int.right;
                case MazeOrientation.West:  return V2Int.left;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }
        
        public static V2Int GetDirectionVector(EMazeMoveDirection _Direction, MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North:
                    switch (_Direction)
                    {
                        case EMazeMoveDirection.Up:    return V2Int.up;
                        case EMazeMoveDirection.Right: return V2Int.right;
                        case EMazeMoveDirection.Down:  return V2Int.down;
                        case EMazeMoveDirection.Left:  return V2Int.left;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                case MazeOrientation.East:
                    switch (_Direction)
                    {
                        case EMazeMoveDirection.Up:    return V2Int.left;
                        case EMazeMoveDirection.Right: return V2Int.up;
                        case EMazeMoveDirection.Down:  return V2Int.right;
                        case EMazeMoveDirection.Left:  return V2Int.down;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                case MazeOrientation.South:
                    switch (_Direction)
                    {
                        case EMazeMoveDirection.Up:    return V2Int.down;
                        case EMazeMoveDirection.Right: return V2Int.left;
                        case EMazeMoveDirection.Down:  return V2Int.up;
                        case EMazeMoveDirection.Left:  return V2Int.right;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                case MazeOrientation.West:
                    switch (_Direction)
                    {
                        case EMazeMoveDirection.Up:    return V2Int.right;
                        case EMazeMoveDirection.Right: return V2Int.down;
                        case EMazeMoveDirection.Down:  return V2Int.left;
                        case EMazeMoveDirection.Left:  return V2Int.up;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }

        public static EMazeMoveDirection GetMoveDirection(V2Int _DirectionVector, MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North:
                    if (_DirectionVector == V2Int.up)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.right)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.down)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.left)
                        return EMazeMoveDirection.Left;
                    break;
                case MazeOrientation.East:
                    if (_DirectionVector == V2Int.left)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.up)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.right)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.down)
                        return EMazeMoveDirection.Left;
                    break;
                case MazeOrientation.South:
                    if (_DirectionVector == V2Int.down)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.left)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.up)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.right)
                        return EMazeMoveDirection.Left;
                    break;
                case MazeOrientation.West:
                    if (_DirectionVector == V2Int.right)
                        return EMazeMoveDirection.Up;
                    else if (_DirectionVector == V2Int.down)
                        return EMazeMoveDirection.Right;
                    else if (_DirectionVector == V2Int.left)
                        return EMazeMoveDirection.Down;
                    else if (_DirectionVector == V2Int.up)
                        return EMazeMoveDirection.Left;
                    break;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
            throw new ArgumentException("Wrong direction vector");
        }

        public static List<V2Int> GetFullPath(V2Int _From, V2Int _To)
        {
            int min, max;
            IEnumerable<int> range;
            IEnumerable<V2Int> result = null;
            
            if (_From.X == _To.X)
            {
                min = Math.Min(_From.Y, _To.Y);
                max = Math.Max(_From.Y, _To.Y);
                range = Enumerable.Range(min, max - min + 1);
                result = range.Select(_V => new V2Int(_From.X, _V));
                if (min == _To.Y) result = result.Reverse();
            }

            if (_From.Y == _To.Y)
            {
                min = Math.Min(_From.X, _To.X);
                max = Math.Max(_From.X, _To.X);
                range = Enumerable.Range(min, max - min + 1);
                result = range.Select(_V => new V2Int(_V, _From.Y));
                if (min == _To.X) result = result.Reverse();
            }
            
            if (result == null)
                throw new ArgumentException($"Cannot build direct path from {_From} to {_To}");
            return result.ToList();
        }

        public static bool PathContainsItem(V2Int _From, V2Int _To, V2Int _Point)
        {
            var fullPath = GetFullPath(_From, _To);
            return fullPath.Contains(_Point);
        }

        /// <summary>
        /// returns -1 if _A less than _B, 0 if _A equals _B, 1 if _A greater than _B
        /// </summary>
        /// <param name="_From">start path element</param>
        /// <param name="_To">end path element</param>
        /// <param name="_A">first path item</param>
        /// <param name="_B">second path item</param>
        /// <returns></returns>
        public static int CompareItemsOnPath(V2Int _From, V2Int _To, V2Int _A, V2Int _B)
        {
            var fullPath = GetFullPath(_From, _To);
            if (!fullPath.Contains(_A))
                throw new ArgumentException($"Path from {_From} to {_To} does not contain point _A {_A}");
            if (!fullPath.Contains(_B))
                throw new ArgumentException($"Path from {_From} to {_To} does not contain point _B {_B}");
            if (_A == _B)
                return 0;
            float distA = Vector2.Distance(_From.ToVector2(), _A.ToVector2());
            float distB = Vector2.Distance(_From.ToVector2(), _B.ToVector2());
            return distA < distB ? -1 : 1;
        }

        public static void DoAppearTransitionSimple(
            bool _Appear,
            IGameTimeProvider _GameTimeProvider,
            Dictionary<IEnumerable<ShapeRenderer>, Color> _Sets,
            UnityAction _OnFinish = null)
        {
            const float transitionTime = 1f;
            foreach (var set in _Sets)
            {
                var startCol = _Appear ? set.Value.SetA(0f) : set.Value;
                var endCol = !_Appear ? set.Value.SetA(0f) : set.Value;
                var shapes = set.Key.Where(_Shape => _Shape != null && !_Shape.IsNull()).ToList();
                if (_Appear)
                    foreach (var shape in shapes)
                        shape.enabled = true;
                Coroutines.Run(Coroutines.Lerp(
                    0f,
                    1f,
                    transitionTime,
                    _Progress =>
                    {
                        foreach (var shape in shapes)
                            shape.Color = Color.Lerp(startCol, endCol, _Progress);
                    },
                    _GameTimeProvider,
                    (_Finished, _Progress) =>
                    {
                        if (!_Appear)
                            foreach (var shape in shapes)
                                shape.enabled = false;
                        _OnFinish?.Invoke();
                    }));
            }
        }

        public static void DoAppearTransitionSimple(
            bool _Appear,
            IGameTimeProvider _GameTimeProvider,
            Dictionary<IEnumerable<Renderer>, Color> _Sets)
        {
            const float transitionTime = 1f;
            foreach (var set in _Sets)
            {
                var startCol = _Appear ? set.Value.SetA(0f): set.Value;
                var endCol = !_Appear ? set.Value.SetA(0f) : set.Value;
                var shapes = set.Key.Where(_Shape => !_Shape.IsNull()).ToList();
                if (_Appear)
                    foreach (var shape in shapes)
                        shape.enabled = true;
                Coroutines.Run(Coroutines.Lerp(
                    0f,
                    1f,
                    transitionTime,
                    _Progress =>
                    {
                        foreach (var shape in shapes)
                        {
                            if (shape is SpriteRenderer spriteRenderer)
                                spriteRenderer.color = Color.Lerp(startCol, endCol, _Progress);
                            else if (shape is MeshRenderer meshRenderer)
                            {
                                foreach (var material in meshRenderer.materials)
                                    material.color = Color.Lerp(startCol, endCol, _Progress);
                            }
                        }
                    },
                    _GameTimeProvider,
                    (_Finished, _Progress) =>
                    {
                        if (!_Appear)
                            foreach (var shape in shapes)
                                shape.enabled = false;
                    }));
            }
        }
        
        #endregion
    }
}
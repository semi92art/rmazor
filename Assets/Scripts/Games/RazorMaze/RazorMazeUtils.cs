using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models;
using UnityEngine.EventSystems;

namespace Games.RazorMaze
{
    public static class RazorMazeUtils
    {
        public static bool IsValidPositionForMove(MazeInfo _Info, MazeItem _MazeItem, V2Int _Position)
        {
            bool isOnNode = _Info.Path.Any(_N => _N == _Position);
            var blockItems = GetBlockMazeItems(_Info.MazeItems);
            bool isOnBlockItem = blockItems.Any(_N => _N.Position == _Position);
            return isOnNode && !isOnBlockItem || _MazeItem.Path.Contains(_Position);
        }
        
        public static IEnumerable<MazeItem> GetBlockMazeItems(IEnumerable<MazeItem> _Items)
        {
            return _Items.Where(_Item =>
                _Item.Type == EMazeItemType.Block
                || _Item.Type == EMazeItemType.Turret
                || _Item.Type == EMazeItemType.TrapReact
                || _Item.Type == EMazeItemType.TrapIncreasing
                || _Item.Type == EMazeItemType.TurretRotating
                || _Item.Type == EMazeItemType.BlockMovingGravity
                || _Item.Type == EMazeItemType.BlockTransformingToNode);
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
        
        public static V2Int GetDirectionVector(MazeMoveDirection _Direction, MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North:
                    switch (_Direction)
                    {
                        case MazeMoveDirection.Up:    return V2Int.up;
                        case MazeMoveDirection.Right: return V2Int.right;
                        case MazeMoveDirection.Down:  return V2Int.down;
                        case MazeMoveDirection.Left:  return V2Int.left;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                case MazeOrientation.East:
                    switch (_Direction)
                    {
                        case MazeMoveDirection.Up:    return V2Int.left;
                        case MazeMoveDirection.Right: return V2Int.up;
                        case MazeMoveDirection.Down:  return V2Int.right;
                        case MazeMoveDirection.Left:  return V2Int.down;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                case MazeOrientation.South:
                    switch (_Direction)
                    {
                        case MazeMoveDirection.Up:    return V2Int.down;
                        case MazeMoveDirection.Right: return V2Int.left;
                        case MazeMoveDirection.Down:  return V2Int.up;
                        case MazeMoveDirection.Left:  return V2Int.right;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                case MazeOrientation.West:
                    switch (_Direction)
                    {
                        case MazeMoveDirection.Up:    return V2Int.right;
                        case MazeMoveDirection.Right: return V2Int.down;
                        case MazeMoveDirection.Down:  return V2Int.left;
                        case MazeMoveDirection.Left:  return V2Int.up;
                        default: throw new SwitchCaseNotImplementedException(_Direction);
                    }
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models;
using UnityEngine;

namespace Games.RazorMaze.Prot
{
    public static class RazorMazePrototypingUtils
    {
        public static List<MazeProtItem> CreateMazeItems(MazeInfo _Info, Transform _Parent)
        {
            var res = new List<MazeProtItem>();
            var converter = new CoordinateConverter();
            converter.Init(_Info.Width);
            _Parent.SetPosXY(converter.GetCenter());
            foreach (var node in _Info.Nodes)
                AddNode(res, node.Position, _Parent, _Info.Width);
            foreach (var obstacle in _Info.Obstacles)
                AddObstacle(res, GetObstacleType(obstacle.Type), obstacle.Position, obstacle.Path, _Parent, _Info.Width);
            return res;
        }

        private static void AddNode(
            ICollection<MazeProtItem> _Items,
            V2Int _Position, 
            Transform _Parent, 
            int _Size)
        {
            bool isFirst = _Items.All(_Item => _Item.Type != MazeItemType.NodeStart);
            var tr = new GameObject("Node").transform;
            tr.SetParent(_Parent);
            var item = tr.gameObject.AddComponent<MazeProtItem>();
            var props = new PrototypingItemProps
            {
                Type = isFirst ? MazeItemType.NodeStart : MazeItemType.Node,
                Size = _Size,
                Position = _Position
            };
            item.Init(props);
            _Items.Add(item);
        }

        private static void AddObstacle(
            ICollection<MazeProtItem> _Items,
            MazeItemType _Type,
            V2Int _Position,
            List<V2Int> _Path,
            Transform _Parent,
            int _Size)
        {
            var tr = new GameObject("Obstacle").transform;
            tr.SetParent(_Parent);
            var item = tr.gameObject.AddComponent<MazeProtItem>();
            var props = new PrototypingItemProps
            {
                Type = _Type,
                Size = _Size,
                Position = _Position,
                Path = _Path
            };
            item.Init(props);
            _Items.Add(item);
        }
        
        public static MazeItemType GetObstacleType(EObstacleType _Type)
        {
            switch (_Type)
            {
                case EObstacleType.Obstacle:        return MazeItemType.Obstacle;
                case EObstacleType.ObstacleMoving:  return MazeItemType.ObstacleMoving;
                case EObstacleType.Trap:            return MazeItemType.ObstacleTrap;
                case EObstacleType.TrapMoving:      return MazeItemType.ObstacleTrapMoving;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        public static EObstacleType GetObstacleType(MazeItemType _Type)
        {
            switch (_Type)
            {
                case MazeItemType.Obstacle:           return EObstacleType.Obstacle;
                case MazeItemType.ObstacleMoving:     return EObstacleType.ObstacleMoving;
                case MazeItemType.ObstacleTrap:       return EObstacleType.Trap;
                case MazeItemType.ObstacleTrapMoving: return EObstacleType.TrapMoving;
            }
            return default;
        }
    }
}
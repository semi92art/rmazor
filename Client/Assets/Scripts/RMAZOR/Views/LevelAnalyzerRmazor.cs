using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views
{
    public interface ILevelAnalyzerRmazor
    {
        bool        IsValid(MazeInfo               _LevelInfo);
        List<V2Int> GetPassMoveDirections(MazeInfo _Info);
    }
    
    public class LevelAnalyzerRmazor : ILevelAnalyzerRmazor
    {
        #region types
        
        private class PathSolve
        {
            public V2Int Position { get; }
            public bool  Solved   { get; set; }

            public PathSolve(V2Int _Pos) => Position = _Pos;
        }

        #endregion
        
        #region api
        
        public bool IsValid(MazeInfo _Info)
        {
            var startPositionsToValidate = GetStartPositionsToValidate(_Info);
            return startPositionsToValidate.All(_StartPosition => IsValid(_Info, _StartPosition, false, out _));
        }

        public List<V2Int> GetPassMoveDirections(MazeInfo _Info)
        {
            List<V2Int> directions;
            while (!IsValid(
                _Info, 
                _Info.PathItems.First().Position,
                true,
                out directions)) {}
            return directions;
        }

        #endregion

        #region nonpublic methods
        
        private static bool IsValid(
            MazeInfo        _Info,
            V2Int           _StartPosition,
            bool            _WritePassDirections,
            out List<V2Int> _Directions)
        {
            _Directions = new List<V2Int>();
            var pathItems = _Info.PathItems;
            if (!pathItems.Any())
                return false;
            var solves = pathItems.Select(_N => new PathSolve(_N.Position)).ToList();
            var currPathItemPos = _StartPosition;
            solves[0].Solved = true;

            int triesCount = _Info.Size.X * _Info.Size.Y;
            int k = 0;
            while (k++ < triesCount)
            {
                var path = pathItems.Select(_PI => _PI.Position).ToList();
                var prevPos = currPathItemPos;
                FollowRandomDirection(ref currPathItemPos, solves, path);
                if (_WritePassDirections)
                    _Directions.Add((currPathItemPos - prevPos).NormalizedAlt);
                if (solves.All(_N => _N.Solved))
                    return true;
            }
            return false;
        }
        
        private static void FollowRandomDirection(
            ref V2Int                      _Position, 
            IReadOnlyCollection<PathSolve> _Solves,
            IReadOnlyCollection<V2Int>     _PathItems)
        {
            var direction = FindDirection(_Position, _PathItems);
            var nextPathItemPos = _Position;
            while (true)
            {
                nextPathItemPos += direction;
                var ns = _Solves.FirstOrDefault(_S => _S.Position == nextPathItemPos);
                if (ns == null)
                    break;
                ns.Solved = true;
            }
            _Position = nextPathItemPos - direction;
        }

        private static V2Int FindDirection(V2Int _Position, IReadOnlyCollection<V2Int> _PathItems)
        {
            var dirs = new List<Vector2Int>
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left
            };
            var dirIdxs = Enumerable.Range(0, dirs.Count).ToList();
            V2Int nextPathItemPos = default;
            while (dirIdxs.Any())
            {
                int dirIdx = Mathf.FloorToInt(MathUtils.RandomGen.NextFloat() * dirs.Count * 0.999f);
                nextPathItemPos = _Position + new V2Int(dirs[dirIdx]);
                dirIdxs.Remove(dirIdx);
                if (!_PathItems.Contains(nextPathItemPos)) 
                    continue;
                break;
            }
            return nextPathItemPos - _Position;
        }
        
        private static IEnumerable<V2Int> GetStartPositionsToValidate(MazeInfo _LevelInfo)
        {
            var startPositionsToValidate = new[]
            {
                _LevelInfo.PathItems.First().Position,
                _LevelInfo.PathItems.Last().Position
            };
            return startPositionsToValidate;
        }

        #endregion
    }
}
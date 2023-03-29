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

        #region nonpublic members

        private static readonly Vector2Int[] Directions =
            {Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left};

        #endregion
        
        #region api
        
        public bool IsValid(MazeInfo _Info)
        {
            var startPositionsToValidate = GetStartPositionsToValidate(_Info);
            bool Predicate(V2Int _StartPos) => IsValid(_Info, _StartPos, false, out _);
            bool isValid = startPositionsToValidate.All(Predicate);
            return isValid;
        }

        public List<V2Int> GetPassMoveDirections(MazeInfo _Info)
        {
            List<V2Int> directions = null;
            int triesCount = 10;
            bool Predicate()
            {
                return !IsValid(
                    _Info,
                    _Info.PathItems[0].Position,
                    true,
                    out directions);
            }
            while (triesCount > 0 && Predicate())
            {
                triesCount--;
            }
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
            var solves = pathItems.Select(_N => new PathSolve(_N.Position)).ToArray();
            var currPathItemPos = _StartPosition;
            solves[0].Solved = true;

            int triesCount = _Info.Size.X * _Info.Size.Y;
            int k = 0;
            while (k++ < triesCount)
            {
                var pathPositions = pathItems.Select(_PI => _PI.Position).ToArray();
                var prevPos = currPathItemPos;
                FollowRandomDirection(ref currPathItemPos, solves, pathPositions);
                if (_WritePassDirections)
                    _Directions.Add((currPathItemPos - prevPos).NormalizedAlt);
                if (solves.All(_N => _N.Solved))
                    return true;
            }
            return false;
        }
        
        private static void FollowRandomDirection(
            ref V2Int   _Position,
            PathSolve[] _Solves,
            V2Int[]     _PathPositions)
        {
            var direction = FindDirection(_Position, _PathPositions);
            var nextPathItemPos = _Position;
            while (true)
            {
                nextPathItemPos += direction;
                PathSolve ns = null;
                for (int i = 0; i < _Solves.Length; i++)
                {
                    if (_Solves[i].Position != nextPathItemPos)
                        continue;
                    ns = _Solves[i];
                }
                if (ns == null)
                    break;
                ns.Solved = true;
            }
            _Position = nextPathItemPos - direction;
        }

        private static V2Int FindDirection(V2Int _Position, V2Int[] _PathPositions)
        {
            var dirIdxs = new List<int> {0, 1, 2, 3};
            int count = 4;
            V2Int nextPathItemPos = default;
            while (count > 0)
            {
                float rand01 = MathUtils.RandomGen.NextFloat();
                int idx = Mathf.FloorToInt(rand01 * count * (1f - float.Epsilon));
                int dirIdx = dirIdxs[idx];
                nextPathItemPos = _Position + new V2Int(Directions[dirIdx]);
                dirIdxs.Remove(dirIdx);
                count--;
                bool contains = false;
                for (int i = 0; i < _PathPositions.Length; i++)
                {
                    if (_PathPositions[i] != nextPathItemPos)
                        continue;
                    contains = true;
                    break;
                }
                if (!contains) 
                    continue;
                break;
            }
            return nextPathItemPos - _Position;
        }
        
        private static IEnumerable<V2Int> GetStartPositionsToValidate(MazeInfo _LevelInfo)
        {
            var startPositionsToValidate = new[]
            {
                _LevelInfo.PathItems[0].Position,
                _LevelInfo.PathItems.Last().Position
            };
            return startPositionsToValidate;
        }

        #endregion
    }
}
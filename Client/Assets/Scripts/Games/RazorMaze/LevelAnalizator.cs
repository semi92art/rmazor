using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Games.RazorMaze.Models;
using UnityEngine;
using Random = UnityEngine.Random;
using Entities;

namespace Games.RazorMaze
{
    public static class LevelAnalizator
    {
        private class PathSolve
        {
            public V2Int Position { get; }
            public bool Solved { get; set; }

            public PathSolve(V2Int _Pos) => Position = _Pos;
        }
        
        public static bool IsValid(MazeInfo _Info, bool _FastValidation = true)
        {
            var sw = Stopwatch.StartNew();
            var pathItems = _Info.Path;
            if (!pathItems.Any())
                return false;
            var solves = pathItems.Select(_N => new PathSolve(_N)).ToList();
            var currPathItemPos = solves[0].Position;
            solves[0].Solved = true;

            int triesCount = _Info.Size.X * _Info.Size.Y;
            if (!_FastValidation)
                triesCount *= 1000;
            int k = 0;
            while (k++ < triesCount)
            {
                FollowRandomDirection(ref currPathItemPos, solves, pathItems);
                if (solves.Any(_N => !_N.Solved)) 
                    continue;
                sw.Stop();
                //Utils.Dbg.Log($"Time elapsed: {sw.Elapsed.TotalMilliseconds}");
                return true;

            }
            sw.Stop();
            //Utils.Dbg.Log($"Time elapsed: {sw.Elapsed.TotalMilliseconds}");
            return false;
        }
        
        
        private static void FollowRandomDirection(
            ref V2Int _Position, 
            IReadOnlyCollection<PathSolve> _Solves,
            List<V2Int> _PathItems)
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
            var dirIdxs = Enumerable.Range(0, LevelGenerator.Directions.Count).ToList();
            V2Int nextPathItemPos = default;
            while (dirIdxs.Any())
            {
                int dirIdx = Mathf.FloorToInt(Random.value * LevelGenerator.Directions.Count * 0.999f);
                nextPathItemPos = _Position + new V2Int(LevelGenerator.Directions[dirIdx]);
                dirIdxs.Remove(dirIdx);
                if (!_PathItems.Contains(nextPathItemPos)) 
                    continue;
                break;
            }
            return nextPathItemPos - _Position;
        }
    }
}
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Games.RazorMaze.Models;
using UnityEngine;
using Random = UnityEngine.Random;
using Entities;

namespace Games.RazorMaze
{
    public static class 
    LevelAnalizator
    {
        private class NodeSolve
        {
            public V2Int Position { get; }
            public bool Solved { get; set; }

            public NodeSolve(V2Int _Node) => Position = _Node;
        }
        
        public static bool IsValid(MazeInfo _Info, bool _FastValidation = true)
        {
            var sw = Stopwatch.StartNew();
            var nodes = _Info.Nodes;
            if (!nodes.Any())
                return false;
            var nodeSolves = nodes.Select(_N => new NodeSolve(_N.Position)).ToList();
            var currNodePos = nodeSolves[0].Position;
            nodeSolves[0].Solved = true;

            int triesCount = _Info.Height * _Info.Width;
            if (!_FastValidation)
                triesCount *= 1000;
            int k = 0;
            while (k++ < triesCount)
            {
                FollowRandomDirection(ref currNodePos, nodeSolves, nodes);
                if (nodeSolves.Any(_N => !_N.Solved)) 
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
            IReadOnlyCollection<NodeSolve> _Solves,
            List<Node> _Nodes)
        {
            var direction = FindDirection(_Position, _Nodes);
            var nextNodePos = _Position;
            while (true)
            {
                nextNodePos += direction;
                var ns = _Solves.FirstOrDefault(_S => _S.Position == nextNodePos);
                if (ns == null)
                    break;
                ns.Solved = true;
            }
            _Position = nextNodePos - direction;
        }

        private static V2Int FindDirection(V2Int _Position, IReadOnlyCollection<Node> _Nodes)
        {
            var dirIdxs = Enumerable.Range(0, LevelGenerator.Directions.Count).ToList();
            V2Int nextNodePos = default;
            while (dirIdxs.Any())
            {
                int dirIdx = Mathf.FloorToInt(Random.value * LevelGenerator.Directions.Count * 0.999f);
                nextNodePos = _Position + new V2Int(LevelGenerator.Directions[dirIdx]);
                dirIdxs.Remove(dirIdx);
                if (!_Nodes.Select(_N => _N.Position).Contains(nextNodePos)) 
                    continue;
                break;
            }
            return nextNodePos - _Position;
        }
    }
}
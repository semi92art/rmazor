using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Games.RazorMaze.Nodes;
using Games.RazorMaze.WallBlocks;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using V2Int = UnityEngine.Vector2Int;

namespace Games.RazorMaze
{
    public class MazeGenerationParams
    {
        public int Width { get; }
        public int Height { get; }
        public float A { get; }
        public int[] PathLengths { get; }

        /// <summary>
        /// Creates RazeMaze Generation Params
        /// </summary>
        /// <param name="_Width">Maze width</param>
        /// <param name="_Height">Maze height</param>
        /// <param name="_A">Walls to nodes ratio, value 0 to 1</param>
        /// <param name="_PathLengths">Path lengths</param>
        public MazeGenerationParams(int _Width, int _Height, float _A,  int[] _PathLengths)
        {
            Width = _Width;
            Height = _Height;
            A = _A;
            PathLengths = _PathLengths;
        }
    }

    public static class LevelGenerator
    {
        public static List<V2Int> Directions => new List<V2Int> {V2Int.up, V2Int.down, V2Int.right, V2Int.left};
        
        public static LevelInfo CreateDefaultLevelInfo(int _Size, bool _OnlyWallBlocks = false)
        {
            return CreateDefaultLevelInfo(_Size, _Size, _OnlyWallBlocks);
        }

        public static LevelInfo CreateDefaultLevelInfo(int _Width, int _Height, bool _OnlyWallBlocks = false)
        {
            GetDefaultNodesAndWallBlockPositions(_Width, _Height, _OnlyWallBlocks,
                out var nodePositions, out var wallBlockPositions);
            return new LevelInfo(
                _Width,
                _Height,
                nodePositions.Select(_Pos => new Node(_Pos) as INode).ToList(), 
                wallBlockPositions.Select(_Pos => new WallBlockSimple(_Pos) as IWallBlock).ToList());
        }
        
        public static LevelInfo CreateRandomLevelInfo(
            MazeGenerationParams _Params,
            out bool _Valid)
        {
            if (!_Params.PathLengths.Any())
            {
                Dbg.LogError("Path lengths array is empty");
                throw new Exception();
            }
            
            int w = _Params.Width;
            int h = _Params.Height;
            GetDefaultNodesAndWallBlockPositions(w, h, true,
                out _, out var wallBlockPositions);
            var nodes = new List<INode>();
            var wallBlocks = wallBlockPositions
                .Select(_P => new WallBlockSimple(_P) as IWallBlock)
                .ToList();
            int xPos = 1 + Mathf.FloorToInt((w - 2) * Random.value);
            int yPos = 1 + Mathf.FloorToInt((h - 2) * Random.value);
            var pos = new V2Int(xPos, yPos);
            nodes.Add(new NodeStart(pos));
            wallBlocks.Remove(wallBlocks.ToList().First(_B => _B.Position == pos));
            int k = 0;
            V2Int dir = GetRandomDirection();
            while (k++ < 1000)
            {
                if (DoBreak(w, h, nodes, _Params.A))
                    break;
                FollowDirectionByLength(
                    ref pos,
                    ref dir,
                    wallBlocks,
                    nodes,
                    _Params,
                    Mathf.RoundToInt(Random.value * _Params.PathLengths.Length * 0.99f - 0.49f));
            }
            foreach (var node in nodes.ToList().
                Where(node => wallBlocks.Any(_B => _B.Position == node.Position)))
                nodes.Remove(node);
            var levelInfo = new LevelInfo(w, h, nodes, wallBlocks);
            _Valid = LevelAnalizator.IsValid(levelInfo);
            return levelInfo;
        }
        
        private static bool DoBreak(
            int _Width,
            int _Height,
            IEnumerable<INode> _AlreadyGenerated, 
            float _A)
        {
            int maxWallBlocksCountWithoutCoeff = _Width * _Height;
            int maxWallBlocksCount = Mathf.RoundToInt(maxWallBlocksCountWithoutCoeff * _A);
            var alreadyGeneratedWallBLocks = _AlreadyGenerated.ToList();
            return alreadyGeneratedWallBLocks.Count >= maxWallBlocksCount;
        }
        
        private static void GetDefaultNodesAndWallBlockPositions(
            int _Width,
            int _Height,
            bool _OnlyWallBlocks,
            out List<V2Int> _NodePositions,
            out List<V2Int> _WallBlockPositions)
        {
            _NodePositions = new List<V2Int>();
            _WallBlockPositions = new List<V2Int>();
            for (int i = 0; i < _Width; i++)
            for (int j = 0; j < _Height; j++)
            {
                var pos = new V2Int(i, j);
                if (i == 0 || i == _Width - 1 || j == 0 || j == _Height - 1 || _OnlyWallBlocks)
                    _WallBlockPositions.Add(pos);
                else
                    _NodePositions.Add(pos);
            }
        }

        private static void FollowDirectionByLength(
            ref V2Int _Position,
            ref V2Int _Direction,
            List<IWallBlock> _WallBlocks,
            List<INode> _Nodes,
            MazeGenerationParams _Params,
            int _PathLenghtIndex)
        {
            var startPos = _Position;
            var dirs = _Direction == V2Int.left || _Direction == V2Int.right
                ? new List<V2Int> {V2Int.up, V2Int.down}
                : new List<V2Int> {V2Int.left, V2Int.right};
            
            while (dirs.Any())
            {
                _Direction = GetNextDirection(ref dirs);
                int nextPosIdx = 0;
                var newNodePositions = new List<V2Int>();
                while (nextPosIdx < _Params.PathLengths[_PathLenghtIndex])
                {
                    _Position += _Direction;
                    var pos = _Position;
                    if (IsPositionOnEdges(_Position, _Params.Width, _Params.Height))
                    {
                        _Position -= _Direction;
                        break;
                    }
                    var block = _WallBlocks.FirstOrDefault(_B => _B.Position == pos);
                    if (block != null)
                    {
                        newNodePositions.Add(pos);
                        _WallBlocks.Remove(block);
                        _Nodes.Add(new Node(_Position));
                    }
                    nextPosIdx++;
                }
                var info = new LevelInfo(_Params.Width, _Params.Height, _Nodes, _WallBlocks);
                bool isValid = LevelAnalizator.IsValid(info);
                Dbg.Log($"Valid: {isValid}");
                if (isValid)
                    break;
                foreach (var pos in newNodePositions)
                {
                    _WallBlocks.Add(new WallBlockSimple(pos));
                    var node = _Nodes.First(_N => _N.Position == pos);
                    _Nodes.Remove(node);
                }
                _Position = startPos;
            }
        }
        
        private static V2Int GetRandomDirection()
        {
            int dirIdx = Mathf.RoundToInt(Random.value * Directions.Count * 0.99f - 0.48f);
            return Directions[dirIdx];
        }

        private static bool IsPositionOnEdges(V2Int _Position, int _Width, int _Height)
        {
            if (_Position.x == 0 || _Position.x == _Width - 1)
                return true;
            if (_Position.y == 0 || _Position.y == _Height - 1)
                return true;
            return false;
        }

        private static V2Int GetNextDirection(ref List<V2Int> _Directions)
        {
            int idx = Mathf.RoundToInt(Random.value * _Directions.Count * 0.99f - 0.49f);
            var dir = _Directions[idx];
            _Directions.Remove(dir);
            return dir;
        }
    }
}

// Vector2 dirIntersectPerimPoint = dir.x > Mathf.Abs(dir.y) ?
// new Vector2(signX * indentX, indentX * dir.y / Mathf.Abs(dir.x)) 
// : new Vector2( indentY * dir.x / Mathf.Abs(dir.y), signY * indentY);
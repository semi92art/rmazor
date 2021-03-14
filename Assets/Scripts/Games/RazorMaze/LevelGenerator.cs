using System;
using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;
using Entities;

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
        public static List<Vector2Int> Directions => new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left
        };
        
        public static MazeInfo CreateDefaultLevelInfo(int _Size, bool _OnlyWallBlocks = false)
        {
            return CreateDefaultLevelInfo(_Size, _Size, _OnlyWallBlocks);
        }

        public static MazeInfo CreateDefaultLevelInfo(int _Width, int _Height, bool _OnlyWallBlocks = false)
        {
            GetDefaultNodesAndWallBlockPositions(_Width, _Height, _OnlyWallBlocks,
                out var nodePositions, out var wallBlockPositions);
            return new MazeInfo
            {
                Width = _Width,
                Height = _Height,
                Nodes = nodePositions.Select(_Pos => new Node{Position = _Pos}).ToList(),
                WallBlocks = wallBlockPositions.Select(_Pos => new WallBlock{Position = _Pos}).ToList(),
                MovingItems = null
            };
        }
        
        public static MazeInfo CreateRandomLevelInfo(
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
            var nodes = new List<Node>();
            var wallBlocks = wallBlockPositions
                .Select(_P => new WallBlock{Position = _P})
                .ToList();
            int xPos = 1 + Mathf.FloorToInt((w - 2) * Random.value);
            int yPos = 1 + Mathf.FloorToInt((h - 2) * Random.value);
            var pos = new V2Int(xPos, yPos);
            nodes.Add(new Node{Position = pos});
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
            var levelInfo = new MazeInfo
            {
                Width = w,
                Height = h,
                Nodes = nodes,
                WallBlocks = wallBlocks,
                MovingItems = null
            };
            _Valid = LevelAnalizator.IsValid(levelInfo);
            return levelInfo;
        }
        
        private static bool DoBreak(
            int _Width,
            int _Height,
            IEnumerable<Node> _AlreadyGenerated, 
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
            List<WallBlock> _WallBlocks,
            List<Node> _Nodes,
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
                        _Nodes.Add(new Node {Position = _Position});
                    }
                    nextPosIdx++;
                }

                var info = new MazeInfo
                {
                    Width = _Params.Width,
                    Height = _Params.Height,
                    Nodes = _Nodes,
                    WallBlocks = _WallBlocks,
                    MovingItems = null
                };
                    
                bool isValid = LevelAnalizator.IsValid(info);
                Dbg.Log($"Valid: {isValid}");
                if (isValid)
                    break;
                foreach (var pos in newNodePositions)
                {
                    _WallBlocks.Add(new WallBlock{Position = pos});
                    var node = _Nodes.First(_N => _N.Position == pos);
                    _Nodes.Remove(node);
                }
                _Position = startPos;
            }
        }
        
        private static V2Int GetRandomDirection()
        {
            int dirIdx = Mathf.RoundToInt(Random.value * Directions.Count * 0.99f - 0.48f);
            return new V2Int(Directions[dirIdx]);
        }

        private static bool IsPositionOnEdges(V2Int _Position, int _Width, int _Height)
        {
            if (_Position.X == 0 || _Position.X == _Width - 1)
                return true;
            if (_Position.Y == 0 || _Position.Y == _Height - 1)
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
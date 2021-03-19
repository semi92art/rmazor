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
        /// <param name="_A">Obstacles to nodes ratio, value 0 to 1</param>
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
        
        public static MazeInfo CreateDefaultLevelInfo(int _Size, bool _OnlyObstacles = false)
        {
            return CreateDefaultLevelInfo(_Size, _Size, _OnlyObstacles);
        }

        public static MazeInfo CreateDefaultLevelInfo(int _Width, int _Height, bool _OnlyObstacles = false)
        {
            GetDefaultNodesAndObstaclesPositions(_Width, _Height, _OnlyObstacles,
                out var nodePositions, out var obstaclePositions);
            return new MazeInfo
            {
                Width = _Width,
                Height = _Height,
                Nodes = nodePositions.Select(_Pos => new Node{Position = _Pos}).ToList(),
                Obstacles = obstaclePositions.Select(_Pos => new Obstacle
                {
                    Position = _Pos, Type = EObstacleType.Obstacle, Path = new List<V2Int>{_Pos}
                }).ToList()
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
            GetDefaultNodesAndObstaclesPositions(w, h, true,
                out _, out var obstaclePositions);
            var nodes = new List<Node>();
            var obstacles = obstaclePositions
                .Select(_P => new Obstacle{Position = _P})
                .ToList();
            int xPos = 1 + Mathf.FloorToInt((w - 2) * Random.value);
            int yPos = 1 + Mathf.FloorToInt((h - 2) * Random.value);
            var pos = new V2Int(xPos, yPos);
            nodes.Add(new Node{Position = pos});
            obstacles.Remove(obstacles.ToList().First(_B => _B.Position == pos));
            int k = 0;
            V2Int dir = GetRandomDirection();
            while (k++ < 1000)
            {
                if (DoBreak(w, h, nodes, _Params.A))
                    break;
                FollowDirectionByLength(
                    ref pos,
                    ref dir,
                    obstacles,
                    nodes,
                    _Params,
                    Mathf.RoundToInt(Random.value * _Params.PathLengths.Length * 0.99f - 0.49f));
            }
            foreach (var node in nodes.ToList().
                Where(node => obstacles.Any(_B => _B.Position == node.Position)))
                nodes.Remove(node);
            var levelInfo = new MazeInfo
            {
                Width = w,
                Height = h,
                Nodes = nodes,
                Obstacles = obstacles
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
            int maxObstaclesCountRaw = _Width * _Height;
            int maxObstaclesCount = Mathf.RoundToInt(maxObstaclesCountRaw * _A);
            var alreadyGeneratedObstacles = _AlreadyGenerated.ToList();
            return alreadyGeneratedObstacles.Count >= maxObstaclesCount;
        }
        
        private static void GetDefaultNodesAndObstaclesPositions(
            int _Width,
            int _Height,
            bool _OnlyObstacles,
            out List<V2Int> _NodePositions,
            out List<V2Int> _ObstaclePositions)
        {
            _NodePositions = new List<V2Int>();
            _ObstaclePositions = new List<V2Int>();
            for (int i = 0; i < _Width; i++)
            for (int j = 0; j < _Height; j++)
            {
                var pos = new V2Int(i, j);
                if (i == 0 || i == _Width - 1 || j == 0 || j == _Height - 1 || _OnlyObstacles)
                    _ObstaclePositions.Add(pos);
                else
                    _NodePositions.Add(pos);
            }
        }

        private static void FollowDirectionByLength(
            ref V2Int _Position,
            ref V2Int _Direction,
            List<Obstacle> _Obstacles,
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
                    var block = _Obstacles.FirstOrDefault(_B => _B.Position == pos);
                    if (block != null)
                    {
                        newNodePositions.Add(pos);
                        _Obstacles.Remove(block);
                        _Nodes.Add(new Node {Position = _Position});
                    }
                    nextPosIdx++;
                }

                var info = new MazeInfo
                {
                    Width = _Params.Width,
                    Height = _Params.Height,
                    Nodes = _Nodes,
                    Obstacles = _Obstacles
                };
                    
                bool isValid = LevelAnalizator.IsValid(info);
                if (isValid)
                    break;
                foreach (var pos in newNodePositions)
                {
                    _Obstacles.Add(new Obstacle
                    {
                        Position = pos,
                        Path = new List<V2Int>{pos}
                    });
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
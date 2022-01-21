using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR
{
    public class MazeGenerationParams
    {
        public V2Int Size { get; }
        public float A { get; }
        public int[] PathLengths { get; }

        /// <summary>
        /// Creates RazeMaze Generation Params
        /// </summary>
        /// <param name="_Size">Maze size, X - width and Y - height</param>
        /// <param name="_A">Maze items to path items ratio, value 0 to 1</param>
        /// <param name="_PathLengths">Path lengths</param>
        public MazeGenerationParams(V2Int _Size, float _A,  int[] _PathLengths)
        {
            Size = _Size;
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
        
        public static MazeInfo CreateDefaultLevelInfo(V2Int _Size, bool _OnlyMazeItems = false)
        {
            GetDefaultPathItemsAndMazeItemsPositions(_Size, _OnlyMazeItems,
                out var pathItemsPositions, out var mazeItemsPositions);
            return new MazeInfo
            {
                Size = _Size,
                PathItems = pathItemsPositions.Select(_P => new PathItem {Position = _P}).ToList(),
                MazeItems = mazeItemsPositions.Select(_Pos => new MazeItem
                {
                    Position = _Pos, Type = EMazeItemType.Block, Path = new List<V2Int>{_Pos}
                }).ToList(),
                AdditionalInfo = new AdditionalInfo
                {
                    Comment1 = "[Empty]",
                    Comment2 = "[Empty]"
                }
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
            
            int w = _Params.Size.X;
            int h = _Params.Size.Y;
            GetDefaultPathItemsAndMazeItemsPositions(_Params.Size, true,
                out _, out var mazeItemsPositions);
            var pathItems = new List<V2Int>();
            var mazeItems = mazeItemsPositions
                .Select(_P => new MazeItem{Position = _P})
                .ToList();
            int xPos = 1 + Mathf.FloorToInt((w - 2) * Random.value);
            int yPos = 1 + Mathf.FloorToInt((h - 2) * Random.value);
            var pos = new V2Int(xPos, yPos);
            pathItems.Add(pos);
            mazeItems.Remove(mazeItems.ToList().First(_B => _B.Position == pos));
            int k = 0;
            V2Int dir = GetRandomDirection();
            while (k++ < 1000)
            {
                if (DoBreak(w, h, pathItems, _Params.A))
                    break;
                FollowDirectionByLength(
                    ref pos,
                    ref dir,
                    mazeItems,
                    pathItems,
                    _Params,
                    Mathf.RoundToInt(Random.value * _Params.PathLengths.Length * 0.99f - 0.49f));
            }
            foreach (var pathItem in pathItems.ToList().
                Where(_PathItem => mazeItems.Any(_B => _B.Position == _PathItem)))
                pathItems.Remove(pathItem);
            var levelInfo = new MazeInfo
            {
                Size = _Params.Size,
                PathItems = pathItems.Select(_P => new PathItem {Position = _P}).ToList(),
                MazeItems = mazeItems
            };
            _Valid = LevelAnalizator.IsValid(levelInfo);
            return levelInfo;
        }
        
        private static bool DoBreak(
            int _Width,
            int _Height,
            IEnumerable<V2Int> _AlreadyGenerated, 
            float _A)
        {
            int maxPathItemsCount = Mathf.RoundToInt(_Width * _Height * _A);
            var alreadyGeneratedPathItems = _AlreadyGenerated.ToList();
            return alreadyGeneratedPathItems.Count >= maxPathItemsCount;
        }
        
        private static void GetDefaultPathItemsAndMazeItemsPositions(
            V2Int _Size,
            bool _OnlyMazeItems,
            out List<V2Int> _PathItems,
            out List<V2Int> _MazeItemsPositions)
        {
            _PathItems = new List<V2Int>();
            _MazeItemsPositions = new List<V2Int>();
            for (int i = 0; i < _Size.X; i++)
            for (int j = 0; j < _Size.Y; j++)
            {
                var pos = new V2Int(i, j);
                if (i == 0 || i == _Size.X - 1 || j == 0 || j == _Size.Y - 1 || _OnlyMazeItems)
                    _MazeItemsPositions.Add(pos);
                else
                    _PathItems.Add(pos);
            }
        }

        private static void FollowDirectionByLength(
            ref V2Int _Position,
            ref V2Int _Direction,
            List<MazeItem> _MazeItems,
            List<V2Int> _Path,
            MazeGenerationParams _Params,
            int _PathLenghtIndex)
        {
            var startPos = _Position;
            var dirs = _Direction == V2Int.Left || _Direction == V2Int.Right
                ? new List<V2Int> {V2Int.Up, V2Int.Down}
                : new List<V2Int> {V2Int.Left, V2Int.Right};
            
            while (dirs.Any())
            {
                _Direction = GetNextDirection(ref dirs);
                int nextPosIdx = 0;
                var newPathItems = new List<V2Int>();
                while (nextPosIdx < _Params.PathLengths[_PathLenghtIndex])
                {
                    _Position += _Direction;
                    var pos = _Position;
                    if (IsPositionOnEdges(_Position, _Params.Size))
                    {
                        _Position -= _Direction;
                        break;
                    }
                    var mazeItem = _MazeItems.FirstOrDefault(_B => _B.Position == pos);
                    if (mazeItem != null)
                    {
                        newPathItems.Add(pos);
                        _MazeItems.Remove(mazeItem);
                        _Path.Add(_Position);
                    }
                    nextPosIdx++;
                }

                var info = new MazeInfo
                {
                    Size = _Params.Size,
                    PathItems = _Path.Select(_P => new PathItem {Position = _P}).ToList(),
                    MazeItems = _MazeItems
                };
                    
                bool isValid = LevelAnalizator.IsValid(info);
                if (isValid)
                    break;
                foreach (var pos in newPathItems)
                {
                    _MazeItems.Add(new MazeItem
                    {
                        Position = pos,
                        Path = new List<V2Int>{pos}
                    });
                    var item = _Path.First(_PathItem => _PathItem == pos);
                    _Path.Remove(item);
                }
                _Position = startPos;
            }
        }
        
        private static V2Int GetRandomDirection()
        {
            int dirIdx = Mathf.RoundToInt(Random.value * Directions.Count * 0.99f - 0.48f);
            return new V2Int(Directions[dirIdx]);
        }

        private static bool IsPositionOnEdges(V2Int _Position, V2Int _Size)
        {
            if (_Position.X == 0 || _Position.X == _Size.X - 1)
                return true;
            if (_Position.Y == 0 || _Position.Y == _Size.Y - 1)
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
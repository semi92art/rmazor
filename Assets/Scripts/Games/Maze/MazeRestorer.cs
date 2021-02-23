using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Games.Maze.Utils;
using UnityEngine;
using UnityEngine.ProBuilder;
using Utils;

namespace Games.Maze
{
    public class MazeRestorer
    {
        public static List<Line2dPoints> RestoreRemainingLines(List<Line2dPoints> _Lines, float _Step)
        {
            var result = _Lines.ToList();
            foreach (var line in _Lines.ToList())
            {
                int steps = Mathf.RoundToInt(Vector2.Distance(line.Start, line.End) / _Step);
                if (steps <= 1) 
                    continue;
                var addPts = new List<Vector2>();
                addPts.Add(line.Start);
                Vector2 addPt = line.Start;
                Vector2 dir = (line.End - line.Start).normalized;
                while (!MazePointsComparer.SamePoints(addPt, line.End))
                {
                    addPt += _Step * dir;
                    addPts.Add(addPt);
                }
                var addLines = new List<Line2dPoints>();
                for (int i = 1; i < addPts.Count; i++)
                    addLines.Add(new Line2dPoints(addPts[i - 1], addPts[i]));

                result.Remove(line);
                result.AddRange(addLines);
            }
            return result;
        }
        
        public static List<Line2dPoints> RestorePathNet(
            MazeCellsType _ShapeType,
            MazeCellsType _CellsType,
            List<Line2dPoints> _Walls,
            Line2dPoints _Start)
        {
            float pathStep = _Walls.Min(_W => Vector2.Distance(_W.Start, _W.End));
            if (_CellsType == MazeCellsType.Triangular)
                pathStep *= 0.5f;
            float wallStep = _CellsType == MazeCellsType.Hexagonal ? pathStep * 0.5f : pathStep;
            var result = new List<Line2dPoints>();
            var wallsDetailed = RestoreRemainingLines(_Walls, wallStep);
            var directions = GetDirections(_CellsType);
            var outerPolygon = GetOuterPolygon(wallsDetailed, _ShapeType);
            RestorePathNetCore(_CellsType, _Start, directions, pathStep, wallsDetailed, outerPolygon, ref result);
            result = result.Distinct(new MazeLinesComparer()).ToList();
            return result;
        }

        private static void RestorePathNetCore(
            MazeCellsType _CellsType,
            Line2dPoints _Start,
            Vector2[] _Directions,
            float _Step,
            List<Line2dPoints> _Walls,
            Vector2[] _OuterPolygon,
            ref List<Line2dPoints> _Lines)
        {
            var joints = new List<Vector2>();
            foreach (var dir in _Directions)
            {
                var pt = _Start.End + dir * _Step;
                if (_Walls.Any(_W => 
                    MazePointsComparer.SamePoints(_W.Start, pt) 
                    || MazePointsComparer.SamePoints(_W.End, pt)))
                    continue;
                Vector2 intersect = default;
                if (_Walls.Any(_W => GeometryUtils.LineSegmentsIntersect(
                    _W.Start, _W.End, _Start.End, pt, ref intersect)))
                    continue;
                if (!GeometryUtils.IsPointInPolygon(_OuterPolygon, pt))
                    continue;
                if (MazePointsComparer.SamePoints(pt, _Start.Start))
                    continue;
                if (_CellsType == MazeCellsType.Hexagonal)
                {
                    Vector2 lineCenter = (pt + _Start.End) * 0.5f;
                    if (_Walls.Any(_W => 
                        Vector2.Distance(lineCenter, _W.Start) < _W.Length * 0.6f
                        || Vector2.Distance(lineCenter, _W.End) < _W.Length * 0.6f))
                        continue;    
                }
                joints.Add(pt);
            }

            foreach (var joint in joints)
            {
                var line = new Line2dPoints(_Start.End, joint);
                if (_Lines.Any(_L => 
                    MazePointsComparer.SamePoints(_L.Start, line.Start)
                    && MazePointsComparer.SamePoints(_L.End, line.End)))
                    continue;
                _Lines.Add(line);
                RestorePathNetCore(_CellsType, line, _Directions, _Step, _Walls, _OuterPolygon, ref _Lines);
            }
        }

        private static Vector2[] GetDirections(MazeCellsType _CellsType)
        {
            switch (_CellsType)
            {
                case MazeCellsType.Rectangular:
                    return new[]
                    {
                        Vector2.up, Vector2.down,
                        Vector2.left, Vector2.right
                    };
                case MazeCellsType.Triangular:
                    float cos60 = Mathf.Cos(60f * Mathf.Deg2Rad);
                    float sin60 = Mathf.Sin(60f * Mathf.Deg2Rad);
                    return new[]
                    {
                        Vector2.right, Vector2.left,
                        new Vector2(cos60, sin60), 
                        new Vector2(cos60, -sin60),
                        new Vector2(-cos60, sin60),
                        new Vector2(-cos60, -sin60)
                    };
                case MazeCellsType.Hexagonal:
                    float sqr3Half = Mathf.Sqrt(3) * 0.5f;
                    float cos30 = Mathf.Cos(30f * Mathf.Deg2Rad);
                    float sin30 = Mathf.Sin(30f * Mathf.Deg2Rad);
                    return new[]
                    {
                        Vector2.up * sqr3Half, Vector2.down * sqr3Half,
                        new Vector2(cos30, sin30) * sqr3Half, 
                        new Vector2(cos30, -sin30) * sqr3Half,
                        new Vector2(-cos30, sin30) * sqr3Half,
                        new Vector2(-cos30, -sin30) * sqr3Half
                    };
                default:
                    throw new SwitchCaseNotImplementedException(_CellsType);
            }
        }

        private static Vector2[] GetOuterPolygon(List<Line2dPoints> _Walls, MazeCellsType _ShapeType)
        {
            float minX, minY, maxX, maxY;
            minX = minY = float.PositiveInfinity;
            maxX = maxY = float.NegativeInfinity;
            
            var wallPts = _Walls
                .SelectMany(_W => new[] {_W.Start, _W.End})
                .Distinct(new MazePointsComparer()).ToList();
            
            foreach (var pt in wallPts)
            {
                if (pt.x < minX) minX = pt.x;
                if (pt.y < minY) minY = pt.y;
                if (pt.x > maxX) maxX = pt.x;
                if (pt.y > maxY) maxY = pt.y;
            }

            switch (_ShapeType)
            {
                case MazeCellsType.Rectangular:
                    return new[]
                    {
                        new Vector2(minX, minY),
                        new Vector2(minX, maxY),
                        new Vector2(maxX, maxY),
                        new Vector2(maxX, minY)
                    };
                case MazeCellsType.Triangular:
                    return new[]
                    {
                        new Vector2(minX, minY),
                        new Vector2((maxX + minX) * 0.5f, maxY),
                        new Vector2(maxX, minY)
                    };
                case MazeCellsType.Hexagonal:
                    float width = maxX - minX;
                    float height = maxY - minY;
                    return new[]
                    {
                        new Vector2(minX, minY + height / 4f),
                        new Vector2(minX, minY + height * 3f / 4f),
                        new Vector2(minX + width / 2f, maxY),
                        new Vector2(maxX, minY + height * 3f / 4f),
                        new Vector2(maxX, minY + height / 4f),
                        new Vector2(minX + width / 2f, minY) 
                    };
                default:
                    throw new SwitchCaseNotImplementedException(_ShapeType);
            }
        }
    }
}
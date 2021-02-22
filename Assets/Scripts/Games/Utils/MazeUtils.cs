using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Extensions;
using UnityEngine;
using Utils;

namespace Games.Utils
{
    public static class MazeUtils
    {
        public static MazeInfo ParseSvg(string _WallsText, string _PathText)
        {
            MazeType type = MazeType.Triangular;
            var wallsDoc = XDocument.Parse(_WallsText);
            if (wallsDoc.Root == null)
                throw new ArgumentException("Svg text does not contain root node");
            var wallRootElements = wallsDoc.Root.Elements().ToList();
            string desc = wallRootElements.SingleOrDefault(_N => Name(_N) == "desc")?.Value;
            var split = desc.Split(' ');
            int.TryParse(split[0], out int xDims);
            int.TryParse(split[2], out int yDims);
            var wallsRaw = wallRootElements
                .Where(_N => Name(_N) == "g")
                .SelectMany(_N => _N.Elements())
                .Where(_N => Name(_N).InRange("line", "polyline"))
                .Select(NodeToLines);
            wallsRaw = MergeJoints(MergeJoints(wallsRaw));
            var walls = ConvertToSimpleLines(wallsRaw);

            var wallPts = walls
                .SelectMany(_W => new[] {_W.Start, _W.End})
                .Distinct(new PointsComparer());
            var wallXCoords = wallPts.Select(_Wp => _Wp.x);
            var wallYCoords = wallPts.Select(_Wp => _Wp.y);
            float width = wallXCoords.Max() - wallXCoords.Min();
            float height = wallYCoords.Max() - wallYCoords.Min();

            List<MazeGraphNode> pathNodes = null;
            if (_PathText != null)
            {
                var pathDoc = XDocument.Parse(_PathText);
                if (pathDoc.Root == null)
                    throw new ArgumentException("Svg text does not contain root node");
                var pathDocDescendants = pathDoc.Root.Descendants().ToList();
                var pathRaw = pathDocDescendants
                    .Where(_N => Name(_N).InRange("line", "polyline"))
                    .Select(NodeToLines);
                var path = ConvertToSimpleLines(pathRaw);
                var first = path.OrderBy(_P => _P.Start.y).First();
                path.Remove(first);
                var last = path.OrderByDescending(_P => _P.End.y).First();
                path.Remove(last);
                float step = path.Min(_P => Vector2.Distance(_P.Start, _P.End));
                pathNodes = ToGraphNodes(path, step);    
            }
            else
            {
                float rStep = 0;
                if (type == MazeType.Triangular)
                {
                    rStep = walls.Min(_W => Vector2.Distance(_W.Start, _W.End)) * 0.5f;
                }

                var wallsDetailed = RestoreRemainingLines(walls, rStep);
                wallPts = wallsDetailed
                    .SelectMany(_W => new[] {_W.Start, _W.End})
                    .Distinct(new PointsComparer());
                //var lines = GetLines(type, wallPts)
            }
            
            
            
            var res = new MazeInfo
            {
                Type = type,
                XDimensions = xDims,
                YDimensions = yDims, 
                Width = width,
                Height = height,
                Walls = walls,
                PathNodes = pathNodes,
                WallWidth = GetMazeLineWidth(xDims)
            };
            return ScaleToScreen(res);
        }

        private static float GetMazeLineWidth(int _XDimensions)
        {
            float coeff = 0.1f;
            var bounds = GameUtils.GetVisibleBounds();
            return coeff * bounds.size.x / _XDimensions;
        }
        
        private static MazeInfo ScaleToScreen(MazeInfo _Info)
        {
            GetStartPointAndScale(_Info, out var startPoint, out float scale);

            foreach (var wall in _Info.Walls)
            {
                wall.Start *= scale;
                wall.End *= scale;
            }

            var minPoint = Vector2.positiveInfinity;
            foreach (var wall in _Info.Walls)
            {
                if (wall.Start.x <= minPoint.x && wall.Start.y <= minPoint.y)
                    minPoint = wall.Start;
                if (wall.End.x <= minPoint.x && wall.End.y <= minPoint.y)
                    minPoint = wall.End;
            }

            foreach (var wall in _Info.Walls)
            {
                wall.Start += startPoint - minPoint;
                wall.End += startPoint - minPoint;
            }

            foreach (var pathNode in _Info.PathNodes)
            {
                pathNode.Point *= scale;
                pathNode.Neighbours = pathNode.Neighbours.Select(_N => _N * scale).ToList();
                pathNode.Point += startPoint - minPoint;
                pathNode.Neighbours = pathNode.Neighbours.Select(_N => _N + startPoint - minPoint).ToList();
            }
            
            return _Info;
        }
        
        private static void GetStartPointAndScale(MazeInfo _Info, out Vector2 _StartPoint, out float _Scale)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float screenRatio = bounds.size.x / bounds.size.y;
            float ratio = _Info.Width / _Info.Height;
            if (ratio > screenRatio)
            {
                float height = bounds.size.y * screenRatio / ratio;
                float posY = bounds.center.y - height * 0.5f;
                _StartPoint = new Vector2(bounds.min.x, posY);
                _Scale = bounds.size.x / _Info.Width;
            }
            else
            {
                float width = bounds.size.x * ratio / screenRatio;
                float posX = bounds.center.x - width * 0.5f;
                _StartPoint = new Vector2(posX, bounds.min.y);
                _Scale = bounds.size.y / _Info.Height;
            }

            if (_Info.Type == MazeType.Hexagonal)
            {
                _StartPoint = _StartPoint.PlusX(_Scale * _Info.Width * 0.25f / _Info.XDimensions)
                    .PlusY(_Scale * _Info.Height * 0.25f / _Info.YDimensions);
            }
        }

        private static List<Line2dPoints> RestoreRemainingLines(List<Line2dPoints> _Lines, float _Step)
        {
            foreach (var line in _Lines.ToList())
            {
                int steps = Mathf.RoundToInt(Vector2.Distance(line.Start, line.End) / _Step);
                if (steps <= 1) 
                    continue;
                var addPts = new List<Vector2>();
                addPts.Add(line.Start);
                Vector2 addPt = line.Start;
                Vector2 dir = (line.End - line.Start).normalized;
                while (!SamePoints(addPt, line.End))
                {
                    addPt += _Step * dir;
                    addPts.Add(addPt);
                }
                var addLines = new List<Line2dPoints>();
                for (int i = 1; i < addPts.Count; i++)
                    addLines.Add(new Line2dPoints(addPts[i - 1], addPts[i]));

                _Lines.Remove(line);
                _Lines.AddRange(addLines);
            }
            return _Lines;
        }
        
        private static List<Line2dPoints> ConvertToSimpleLines(IEnumerable<PolyLine2dPoints> _Lines)
        {
            var resLines = new List<Line2dPoints>();
            foreach (var pts in _Lines.Select(_Line => _Line.Points))
                for (int i = 1; i < pts.Count; i++)
                    resLines.Add(new Line2dPoints(pts[i - 1], pts[i]));
            return resLines;
        }

        private static IEnumerable<PolyLine2dPoints> MergeJoints(IEnumerable<PolyLine2dPoints> _Lines)
        {
            var mergedLines = new List<PolyLine2dPoints>();
            var excludedLines = new List<PolyLine2dPoints>();
            var linesCopy = _Lines.ToList();
            foreach (var lineA in linesCopy.ToList())
            {
                var firstPtA = lineA.Points.First();
                var lastPtA = lineA.Points.Last();
                foreach (var lineB in linesCopy.ToList())
                {
                    if (ReferenceEquals(lineA, lineB))
                        continue;
                    var firstPtB = lineB.Points.First();
                    var lastPtB = lineB.Points.Last();
                    PolyLine2dPoints[] linesToMerge = null;
                    
                    if (SamePoints(lastPtA, firstPtB))
                        linesToMerge = new[] {lineA, lineB};
                    else if (SamePoints(firstPtA, firstPtB))
                        linesToMerge = new[] {new PolyLine2dPoints(Enumerable.Reverse(lineA.Points).ToList()), lineB};
                    else if (SamePoints(lastPtA, lastPtB))
                        linesToMerge = new[] {lineA, new PolyLine2dPoints(Enumerable.Reverse(lineB.Points).ToList())};
                    
                    if (linesToMerge == null) 
                        continue;
                    
                    if (!excludedLines.Contains(lineA))
                        excludedLines.Add(lineA);
                    if (!excludedLines.Contains(lineB))
                        excludedLines.Add(lineB);
                    
                    linesToMerge[0].Points.RemoveAt(lineA.Points.Count - 1);
                    var mergedPts = linesToMerge[0].Points.Concat(linesToMerge[1].Points).ToList();
                    mergedLines.Add(new PolyLine2dPoints(mergedPts));
                    break;
                }
            }
            return linesCopy.Except(excludedLines).Concat(mergedLines);
        }

        private static List<MazeGraphNode> ToGraphNodes(List<Line2dPoints> _Lines, float _Step)
        {
            var res = new List<MazeGraphNode>();
            var lines = RestoreRemainingLines(_Lines, _Step);
            var pts = lines
                .SelectMany(_L => new[] {_L.Start, _L.End})
                .Distinct();
            foreach (var pt in pts)
            {
                var gNode = new MazeGraphNode {Point = pt, Neighbours = new List<Vector2>()};
                foreach (var line in lines)
                {
                    if (SamePoints(pt, line.Start))
                        gNode.Neighbours.Add(line.End);
                    else if (SamePoints(pt, line.End))
                        gNode.Neighbours.Add(line.Start);
                }
                gNode.Neighbours = gNode.Neighbours.Distinct(new PointsComparer()).ToList();
                res.Add(gNode);
            }
            return res;
        }

        private static PolyLine2dPoints NodeToLines(XElement _Node)
        {
            if (Name(_Node) == "line")
            {
                float x1 = GetPoint(_Node.Attribute("x1")?.Value);
                float y1 = GetPoint(_Node.Attribute("y1")?.Value);
                float x2 = GetPoint(_Node.Attribute("x2")?.Value);
                float y2 = GetPoint(_Node.Attribute("y2")?.Value);
                return new PolyLine2dPoints(new List<Vector2>
                {
                    new Vector2(x1, y1),
                    new Vector2(x2, y2)
                });
            }
            
            if (Name(_Node) == "polyline")
            {
                var pointsAttr = _Node.Attribute("points");
                var ptsStr = pointsAttr?.Value.Split(' ');
                var points = new List<Vector2>();
                foreach (var ptStr in ptsStr)
                {
                    var split = ptStr.Split(',');
                    var point = new Vector2(
                        float.Parse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                        float.Parse(split[1],NumberStyles.Any, CultureInfo.InvariantCulture));
                    points.Add(point);
                }
                return new PolyLine2dPoints(points);
            }
            
            throw new ArgumentException("Node name must be \"line\" or \"polyline\"");
        }

        private static List<Line2dPoints> GetLines(
            MazeType _Type,
            IEnumerable<Vector2> _WallPoints,
            Line2dPoints _FirstPath,
            float _Step,
            Vector4 _MinMaxXY)
        {
            var result = new List<Line2dPoints>();
            Vector2[] directions = null;
            if (_Type == MazeType.Triangular)
                directions = new []
                {
                    Vector2.one, -Vector2.one,
                    Vector2.right, Vector2.left,
                    new Vector2(-1, 1), new Vector2(1, -1)
                };
            GetLinesCore(_FirstPath, directions, _Step, _WallPoints, _MinMaxXY,  ref result);
            return result;
        }

        private static void GetLinesCore(
            Line2dPoints _Line,
            Vector2[] _Directions,
            float _Step,
            IEnumerable<Vector2> _WallPoints,
            Vector4 _MinMaxXY,
            ref List<Line2dPoints> _Lines)
        {
            var joints = new List<Vector2>();
            foreach (var dir in _Directions)
            {
                var pt = _Line.End + dir * _Step;
                if (_WallPoints.Any(_P => SamePoints(_P, pt)))
                    continue;
                if (pt.x < _MinMaxXY.x || pt.x > _MinMaxXY.z)
                    continue;
                if (pt.y < _MinMaxXY.y || pt.y > _MinMaxXY.w)
                    continue;
                if (SamePoints(pt, _Line.Start))
                    continue;
                joints.Add(pt);
            }

            foreach (var joint in joints)
            {
                var line = new Line2dPoints(_Line.End, joint);
                _Lines.Add(line);
                GetLinesCore(line, _Directions, _Step, _WallPoints, _MinMaxXY, ref _Lines);
            }
        }

        private static bool SamePoints(Vector2 _A, Vector2 _B)
        {
            float epsilon = 0.001f;
            return Mathf.Abs(_A.x - _B.x) < epsilon
                   && Mathf.Abs(_A.y - _B.y) < epsilon;
        }

        private static string Name(XElement _Node)
        {
            return _Node.Name.LocalName;
        }

        private static float GetPoint(string _Value)
        {
            float.TryParse(_Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float res);
            return res;
        }
        
        private class PointsComparer : IEqualityComparer<Vector2>
        {
            public bool Equals(Vector2 _A, Vector2 _B) => SamePoints(_A, _B);
            public int GetHashCode(Vector2 _V) => _V.GetHashCode();
        }
    }

    public class Line2dPoints
    {
        public Vector2 Start { get; set; }
        public Vector2 End { get; set; }

        public Line2dPoints(Vector2 _Start, Vector2 _End)
        {
            Start = _Start;
            End = _End;
        }
    }

    public class MazeGraphNode
    {
        public Vector2 Point { get; set; }
        public List<Vector2> Neighbours { get; set; }
    }
    
    public class PolyLine2dPoints
    {
        public List<Vector2> Points { get; }

        public PolyLine2dPoints(List<Vector2> _Points)
        {
            Points = _Points;
        }
    }
    
    public class MazeInfo
    {
        public MazeType Type { get; set; }
        public int XDimensions { get; set; }
        public int YDimensions { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float WallWidth { get; set; }
        public List<Line2dPoints> Walls { get; set; }
        public List<MazeGraphNode> PathNodes { get; set; }
    }

    public enum MazeType
    {
        Rectangular,
        Hexagonal,
        Triangular,
        Circled
    }
}
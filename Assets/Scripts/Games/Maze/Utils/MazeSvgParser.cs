using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Extensions;
using UnityEngine;
using Utils;

namespace Games.Maze.Utils
{
    public static class MazeSvgParser
    {
        private const float WallWidthCoefficient = 0.1f;
        
        public static MazeInfo ParseSvg(string _WallsText)
        {
            MazeCellsType shapeType = MazeCellsType.Rectangular;
            MazeCellsType cellsType = MazeCellsType.Hexagonal;
            
            var wallsDoc = XDocument.Parse(_WallsText);
            if (wallsDoc.Root == null)
                throw new System.ArgumentException("Svg text does not contain root node");
            var wallRootElements = wallsDoc.Root.Elements().ToList();
            string desc = wallRootElements.SingleOrDefault(_N => NName(_N) == "desc")?.Value;
            var split = desc.Split(' ');
            int.TryParse(split[0], out int xDims);
            int.TryParse(split[2], out int yDims);
            var wallsRaw = wallRootElements
                .Where(_N => NName(_N) == "g")
                .SelectMany(_N => _N.Elements())
                .Where(_N => NName(_N).InRange("line", "polyline"))
                .Select(XElementToLines);
            wallsRaw = MergeJoints(MergeJoints(wallsRaw));
            var walls = ConvertToSimpleLines(wallsRaw);

            var wallPts = walls
                .SelectMany(_W => new[] {_W.Start, _W.End})
                .Distinct(new MazePointsComparer());
            var wallXCoords = wallPts.Select(_Wp => _Wp.x);
            var wallYCoords = wallPts.Select(_Wp => _Wp.y);
            float width = wallXCoords.Max() - wallXCoords.Min();
            float height = wallYCoords.Max() - wallYCoords.Min();
            
            XElement pathNode = wallRootElements.SingleOrDefault(_N => NName(_N) == "polyline");
            var path = ConvertToSimpleLines(new[] {XElementToLines(pathNode)});
            var pathNet = MazeRestorer.RestorePathNet(shapeType, cellsType, walls, path[2]);

            float step = pathNet.Min(_P => Vector2.Distance(_P.Start, _P.End));
            var pathNodes = ToGraphNodes(pathNet, step);
            
            var res = new MazeInfo
            {
                CellsType = cellsType,
                Dimensions = new Vector2Int(xDims, yDims),
                Size = new Vector2(width, height),
                Walls = walls,
                PathNodes = pathNodes,
                WallWidth = GetMazeLineWidth(xDims)
            };
            return MazeScreenScaler.ScaleToScreen(res);
        }

        private static float GetMazeLineWidth(int _XDimensions)
        {
            var bounds = GameUtils.GetVisibleBounds();
            return WallWidthCoefficient * bounds.size.x / _XDimensions;
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
                    
                    if (MazePointsComparer.SamePoints(lastPtA, firstPtB))
                        linesToMerge = new[] {lineA, lineB};
                    else if (MazePointsComparer.SamePoints(firstPtA, firstPtB))
                        linesToMerge = new[] {new PolyLine2dPoints(Enumerable.Reverse(lineA.Points).ToList()), lineB};
                    else if (MazePointsComparer.SamePoints(lastPtA, lastPtB))
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
            var lines = MazeRestorer.RestoreRemainingLines(_Lines, _Step);
            var pts = lines
                .SelectMany(_L => new[] {_L.Start, _L.End})
                .Distinct();
            foreach (var pt in pts)
            {
                var gNode = new MazeGraphNode {Point = pt, Neighbours = new List<Vector2>()};
                foreach (var line in lines)
                {
                    if (MazePointsComparer.SamePoints(pt, line.Start))
                        gNode.Neighbours.Add(line.End);
                    else if (MazePointsComparer.SamePoints(pt, line.End))
                        gNode.Neighbours.Add(line.Start);
                }
                gNode.Neighbours = gNode.Neighbours.Distinct(new MazePointsComparer()).ToList();
                res.Add(gNode);
            }
            return res;
        }

        private static PolyLine2dPoints XElementToLines(XElement _Node)
        {
            if (NName(_Node) == "line")
            {
                float x1 = GetSvgFloat(_Node.Attribute("x1")?.Value);
                float y1 = GetSvgFloat(_Node.Attribute("y1")?.Value);
                float x2 = GetSvgFloat(_Node.Attribute("x2")?.Value);
                float y2 = GetSvgFloat(_Node.Attribute("y2")?.Value);
                return new PolyLine2dPoints(new List<Vector2>
                {
                    new Vector2(x1, y1),
                    new Vector2(x2, y2)
                });
            }
            
            if (NName(_Node) == "polyline")
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
            
            throw new System.ArgumentException("Node name must be \"line\" or \"polyline\"");
        }
        
        private static string NName(XElement _Node)
        {
            return _Node.Name.LocalName;
        }

        private static float GetSvgFloat(string _Value)
        {
            float.TryParse(_Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float res);
            return res;
        }
    }
}
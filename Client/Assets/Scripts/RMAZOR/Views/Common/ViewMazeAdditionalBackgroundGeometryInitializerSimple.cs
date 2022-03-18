using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Views.Common
{
    public interface IViewMazeAdditionalBackgroundGeometryInitializer : IInit
    {
        List<PointsGroupArgs> GetGroups(MazeInfo         _Info);
    }
    
    public class ViewMazeAdditionalBackgroundGeometryInitializerSimple 
        : InitBase, IViewMazeAdditionalBackgroundGeometryInitializer
    {
        #region nonpublic members
        
        private List<PointsGroupArgs> m_GroupsCached;

        #endregion
        
        #region api
        
        public List<PointsGroupArgs> GetGroups(MazeInfo _Info = null)
        {
            var points = GetPointsOfPathItemsAndMazeItems(_Info);
            var groups = GetPointGroups(points);
            int idx = 10;
            m_GroupsCached = groups
                .Select(_G =>
                {
                    // var holes = GetGroupHoles(_G);
                    var holes = new List<List<V2Int>>();
                    return new PointsGroupArgs(_G, holes, ++idx);
                })
                .ToList();
            return m_GroupsCached;
        }

        #endregion

        #region nonpublic methods
        
        private static IEnumerable<V2Int> GetPointsOfPathItemsAndMazeItems(MazeInfo _Info)
        {
            return _Info.MazeItems
                .Where(_I => _I.Type != EMazeItemType.Block && _I.Type != EMazeItemType.TrapReact)
                .Select(_I => _I.Position)
                .Concat(_Info.PathItems.Select(_I => _I.Position))
                .Distinct();
        }

        private static List<List<V2Int>> GetPointGroups(IEnumerable<V2Int> _Points)
        {
            var points = _Points.OrderBy(_P => _P.X).ToList();
            var groups = new List<List<V2Int>>{points};
            return groups;
        }

        private static List<List<V2Int>> GetGroupHoles(IEnumerable<V2Int> _Group)
        {
            var group = _Group.ToList();
            int minX = group.Min(_P => _P.X);
            int minY = group.Min(_P => _P.Y);
            int maxX = group.Max(_P => _P.X);
            int maxY = group.Max(_P => _P.Y);
            var areHolePoints = new bool?[maxX + 1, maxY + 1];
            var surroundingNotMazePoints = GetSurroundingNotMazePoints(group);
            for (int i = minX; i <= maxX; i++)
            for (int j = minY; j <= maxY; j++)
            {
                var pos = new V2Int(i, j);
                if (group.Contains(new V2Int(i, j)) || surroundingNotMazePoints.Contains(pos))
                    continue;
                areHolePoints[i, j] = true;
            }
            
            var holesPoints = new List<V2Int>();
            for (int i = minX; i <= maxX; i++)
            for (int j = minY; j <= maxY; j++)
            {
                var isHolePoint = areHolePoints[i, j];
                if (!isHolePoint.HasValue  || !isHolePoint.Value)
                    continue;
                int joints = 0;
                void CheckForJoint(bool? _Neighbour)
                {
                    if (_Neighbour.HasValue && _Neighbour.Value)
                        joints++;
                }
                if (i > minX) CheckForJoint(areHolePoints[i - 1, j]);
                if (j > minY) CheckForJoint(areHolePoints[i, j - 1]);
                if (i < maxX) CheckForJoint(areHolePoints[i + 1, j]);
                if (j < maxY) CheckForJoint(areHolePoints[i, j + 1]);
                areHolePoints[i, j] = joints >= 2;
                if (areHolePoints[i, j].Value)
                    holesPoints.Add(new V2Int(i, j));
            }
            holesPoints = holesPoints.OrderBy(_P => _P.X).ToList();
            var holes = new List<List<V2Int>>();
            if (!holesPoints.Any())
                return holes;
            while (group.Any())
            {
                minX = holesPoints.First().X;
                minY = holesPoints.Where(_P => _P.X == minX).Min(_P => _P.Y);
                var hole = GetNeighboringPoints(group, new V2Int(minX, minY), false);
                holes.Add(hole);
            }
            return holes;
        }
        
        private static List<V2Int> GetNeighboringPoints(
            ICollection<V2Int> _Points,
            V2Int              _Point,
            bool               _ForHoles)
        {
            IEnumerable<V2Int> result = new List<V2Int> {_Point};
            V2Int? neighboring1,
                   neighboring2,
                   neighboring3,
                   neighboring4 = null,
                   neighboring5 = null,
                   neighboring6 = null,
                   neighboring7 = null,
                   neighboring8 = null,
                   neighboring9 = null;
            V2Int? CheckForNeighbor(V2Int _P)
            {
                if (!_Points.Contains(_P))
                    return null;
                _Points.Remove(_P);
                return _P;
            }
            neighboring1 = CheckForNeighbor(_Point + V2Int.Right);
            neighboring2 = CheckForNeighbor(_Point + V2Int.Up);
            neighboring3 = CheckForNeighbor(_Point + V2Int.Down);
            if (!_ForHoles)
            {
                neighboring4 = CheckForNeighbor(_Point + V2Int.Right + V2Int.Up);
                neighboring5 = CheckForNeighbor(_Point + V2Int.Right + V2Int.Down);
                neighboring6 = CheckForNeighbor(_Point + V2Int.Left + V2Int.Down);
                neighboring7 = CheckForNeighbor(_Point + V2Int.Left + V2Int.Up);
                neighboring9 = CheckForNeighbor(_Point + V2Int.Left);
            }
            var neighbors = result.ToList();
            void ConcatWithNeighbors(IEnumerable<V2Int?> _Neighbors)
            {
                foreach (var n in _Neighbors.Where(_N => _N.HasValue))
                {
                    var neighborNeighbors = GetNeighboringPoints(_Points, n.Value, _ForHoles);
                    neighbors = neighbors.Concat(neighborNeighbors).ToList();
                }
            }
            ConcatWithNeighbors(new [] { neighboring1, neighboring2, neighboring3});
            if (_ForHoles)
                return neighbors.Distinct().ToList();
            ConcatWithNeighbors(new []
            {
                neighboring4, 
                neighboring5,
                neighboring6,
                neighboring7,
                neighboring8,
                neighboring9
            });
            return neighbors.Distinct().ToList();
        }

        private static List<V2Int> GetSurroundingNotMazePoints(ICollection<V2Int> _Group)
        {
            var group = _Group.ToList();
            int minX = group.Min(_P => _P.X);
            int minY = group.Min(_P => _P.Y);
            int maxX = group.Max(_P => _P.X);
            int maxY = group.Max(_P => _P.Y);
            var xPositions = Enumerable.Range(minX, maxX + 1 - minX).ToList();
            var yPositions = Enumerable.Range(minY, maxY + 1 - minY).ToList();
            var surroundingPoints = new List<V2Int>();
            foreach (int xPos in xPositions)
            {
                GetNeighboringSurroundingNotMazePoints(group, surroundingPoints, new V2Int(xPos, minY));
                GetNeighboringSurroundingNotMazePoints(group, surroundingPoints, new V2Int(xPos, maxY));
            }
            foreach (int yPos in yPositions)
            {
                GetNeighboringSurroundingNotMazePoints(group, surroundingPoints, new V2Int(minX, yPos));
                GetNeighboringSurroundingNotMazePoints(group, surroundingPoints, new V2Int(maxX, yPos));
            }
            return surroundingPoints;
        }
        
        private static void GetNeighboringSurroundingNotMazePoints(
            ICollection<V2Int> _Group,
            ICollection<V2Int> _SurroundingPoints,
            V2Int              _Point)
        {
            if (_Group.Contains(_Point) || _SurroundingPoints.Contains(_Point))
                return;
            V2Int? CheckForNeighbor(V2Int _P)
            {
                return _Group.Contains(_Point) || _SurroundingPoints.Contains(_Point) ? (V2Int?)null : _P;
            }
            var neigh1 = CheckForNeighbor(_Point + V2Int.Left + V2Int.Down);
            var neigh2 = CheckForNeighbor(_Point + V2Int.Left);
            var neigh3 = CheckForNeighbor(_Point + V2Int.Left + V2Int.Up);
            var neigh4 = CheckForNeighbor(_Point + V2Int.Up);
            var neigh5 = CheckForNeighbor(_Point + V2Int.Right + V2Int.Up);
            var neigh6 = CheckForNeighbor(_Point + V2Int.Right);
            var neigh7 = CheckForNeighbor(_Point + V2Int.Right + V2Int.Down);
            var neigh8 = CheckForNeighbor(_Point + V2Int.Down);
            var neighbors = new[] {neigh1, neigh2, neigh3, neigh4, neigh5, neigh6, neigh7, neigh8};
            foreach (var neigh in neighbors.Distinct())
            {
                if (neigh.HasValue && !_SurroundingPoints.Contains(neigh.Value))
                    _SurroundingPoints.Add(neigh.Value);
            }
            void ConcatWithNeighbors(IEnumerable<V2Int?> _Neighbors)
            {
                foreach (var n in _Neighbors.Where(_N => _N.HasValue))
                {
                    GetNeighboringSurroundingNotMazePoints(
                        _Group, _SurroundingPoints, n.Value);
                }
            }
            ConcatWithNeighbors(neighbors.Where(_N => _N.HasValue));
        }

        #endregion
    }
}
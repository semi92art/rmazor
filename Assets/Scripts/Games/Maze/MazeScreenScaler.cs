using System.Linq;
using UnityEngine;
using Utils;

namespace Games.Maze
{
    public static class MazeScreenScaler
    {
        private const float BigMazeMinScale = 0.1f;
        private const float HorizontalScreenOffset = 0.2f;
        private const float TopScreenOffset = 1f;
        private const float BottomScreenOffset = 1f;

        public static MazeInfo ScaleToScreen(MazeInfo _Info)
        {
            GetStartPointAndScale(_Info, out var startPoint, out float scale);

            foreach (var wall in _Info.Walls)
            {
                wall.Start *= scale;
                wall.End *= scale;
            }

            var minCoords = GetMinCoords(_Info);
            var startPointDelta = startPoint - minCoords;
            
            foreach (var wall in _Info.Walls)
            {
                wall.Start += startPointDelta;
                wall.End += startPointDelta;
            }

            if (_Info.PathNodes == null) 
                return _Info;
            
            foreach (var pathNode in _Info.PathNodes)
            {
                pathNode.Point *= scale;
                pathNode.Neighbours = pathNode.Neighbours.Select(_N => _N * scale).ToList();
                pathNode.Point += startPointDelta;
                pathNode.Neighbours = pathNode.Neighbours.Select(_N => _N + startPointDelta).ToList();
            }

            return _Info;
        }
        
        private static void GetStartPointAndScale(MazeInfo _Info, out Vector2 _StartPoint, out float _Scale)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float bottomScreenOffset = _Info.IsBig ? 0f : BottomScreenOffset;
            float topScreenOffset = _Info.IsBig ? 0f : TopScreenOffset;
            float realBoundsSizeX = bounds.size.x - HorizontalScreenOffset * 2f;
            float realBoundsSizeY = bounds.size.y - bottomScreenOffset - topScreenOffset;
            float screenRatio = realBoundsSizeX / realBoundsSizeY;
            float mazeRatio = _Info.Size.x / _Info.Size.y;

            float startX, startY;
            if (mazeRatio > screenRatio)
            {
                float height = realBoundsSizeY * screenRatio / mazeRatio;
                startX = bounds.min.x + HorizontalScreenOffset;
                startY = bounds.center.y - height * 0.5f;
                _Scale = realBoundsSizeX / _Info.Size.x;
            }
            else
            {
                float width = realBoundsSizeX * mazeRatio / screenRatio;
                startX = bounds.center.x - width * 0.5f;
                startY = bounds.min.y + BottomScreenOffset;
                _Scale = realBoundsSizeY / _Info.Size.y;
            }

            if (_Info.IsBig)
                _Scale = Mathf.Max(_Scale, BigMazeMinScale);

            _Info.Size *= _Scale;
            _StartPoint = _Info.IsBig ? Vector2.zero : new Vector2(startX, startY);
        }

        private static Vector2 GetMinCoords(MazeInfo _Info)
        {
            float minX, minY;
            minX = minY = float.PositiveInfinity;
            
            foreach (var wall in _Info.Walls)
            {
                minX = Mathf.Min(minX, Mathf.Min(wall.Start.x, wall.End.x));
                minY = Mathf.Min(minY, Mathf.Min(wall.Start.y, wall.End.y));
            }

            return new Vector2(minX, minY);
        }
    }
}
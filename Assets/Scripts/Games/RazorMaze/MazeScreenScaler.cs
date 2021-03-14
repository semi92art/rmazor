using Entities;
using UnityEngine;
using Utils;

namespace Games.RazorMaze
{
    public class MazeScreenScaler
    {
        private float m_HorizontalOffset;
        private float m_TopOffset;
        private float m_BottomOffset;

        public MazeScreenScaler() : this(1f, 1f, 1f) { }
        
        public MazeScreenScaler(float _HorizontalOffset, float _TopOffset, float _BottomOffset)
        {
            m_HorizontalOffset = _HorizontalOffset;
            m_TopOffset = _TopOffset;
            m_BottomOffset = _BottomOffset;
        }
        

        public Vector2 GetWorldPosition(V2Int _Point, int _Width, int _Height, out float _Scale)
        {
            GetStartPointAndScale(_Width, _Height, out var startPoint, out _Scale);
            return _Point.ToVector2() * _Scale + startPoint;
        }
        
        private void GetStartPointAndScale(
            int _Width,
            int _Height,
            out Vector2 _StartPoint,
            out float _Scale)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSizeX = bounds.size.x - m_HorizontalOffset * 2f;
            float realBoundsSizeY = bounds.size.y - m_BottomOffset - m_TopOffset;
            float screenRatio = realBoundsSizeX / realBoundsSizeY;
            float mazeRatio = _Width / (float)_Height;
            float height = realBoundsSizeY * screenRatio / mazeRatio;
            _Scale = realBoundsSizeX / _Width;
            float itemWidth = realBoundsSizeX / _Width;
            float startX = bounds.min.x + m_HorizontalOffset + itemWidth * 0.5f;
            float startY = bounds.center.y - height * 0.5f;
            _StartPoint = new Vector2(startX, startY);
        }
    }
}
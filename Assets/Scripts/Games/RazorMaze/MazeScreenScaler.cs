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

        public MazeScreenScaler() : this(1f, 5f, 10f) { }
        
        private MazeScreenScaler(float _HorizontalOffset, float _TopOffset, float _BottomOffset)
        {
            m_HorizontalOffset = _HorizontalOffset;
            m_TopOffset = _TopOffset;
            m_BottomOffset = _BottomOffset;
        }

        public Vector2 GetWorldPosition(V2Int _Point, int _Size, out float _Scale)
        {
            GetStartPointAndScale(_Size, out var startPoint, out _Scale);
            return _Point.ToVector2() * _Scale + startPoint;
        }
        
        private void GetStartPointAndScale(
            int _Size,
            out Vector2 _StartPoint,
            out float _Scale)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSizeX = bounds.size.x - m_HorizontalOffset * 2f;
            _Scale = realBoundsSizeX / _Size;
            float itemSize = realBoundsSizeX / _Size;
            float startX = bounds.min.x + m_HorizontalOffset + itemSize * 0.5f;

            float startY = bounds.min.y + m_BottomOffset + itemSize * 0.5f;
            //float screenRatio = realBoundsSizeX / realBoundsSizeY;
            //float mazeRatio = _Width / (float)_Height;
            //float height = realBoundsSizeY * screenRatio / mazeRatio;
            //float startY = bounds.center.y - height * 0.5f;
            
            _StartPoint = new Vector2(startX, startY);
        }

        public Vector2 GetCenter(int _Size)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - m_HorizontalOffset * 2f;
            float itemSize = realBoundsSize / _Size;
            float startY = bounds.min.y + m_BottomOffset;
            float centerY = startY + realBoundsSize * 0.5f;
            return new Vector2(bounds.center.x, centerY);
        }
    }
}
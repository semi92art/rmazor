using DI.Extensions;
using Entities;
using UnityEngine;
using Utils;
using Zenject;

namespace Games.RazorMaze
{
    public interface ICoordinateConverter
    {
        V2Int MazeSize { get; set; }
        float Scale { get; }
        Vector2 GetMazeCenter();
        Bounds GetMazeBounds();
        Vector4 GetScreenOffsets();
        Vector2 ToGlobalMazePosition(Vector2 _Point);
        Vector2 ToLocalMazeItemPosition(Vector2 _Point);
        Vector2 ToLocalCharacterPosition(Vector2 _Point);
    }
    
    public class CoordinateConverter : ICoordinateConverter
    {
        #region constants

        private const float LeftOffset = 1f;
        private const float RightOffset = 1f;
        private const float TopOffset = 5f;
        private const float BottomOffset = 10f;


        #endregion
        
        #region nonpublic members


        private V2Int m_MazeSize;
        private float m_Scale;
        private Vector2 m_StartPoint;
        private Vector2 m_Center;
        private bool m_Initialized;

        #endregion
        
        #region api

        [Inject] public CoordinateConverter() => SetCenter();
        

        public V2Int MazeSize
        {
            get => m_MazeSize;
            set
            {
                m_MazeSize = value;
                m_Initialized = true;
                CheckForErrors();
                SetStartPointAndScale();
            }
        }

        public float Scale
        {
            get
            {
                CheckForErrors();
                return m_Scale;
            }
        }

        public Vector2 GetMazeCenter()
        {
            return m_Center;
        }

        public Vector4 GetScreenOffsets()
        {
            return new Vector4(LeftOffset, RightOffset, BottomOffset, TopOffset);
        }
        
        public Bounds GetMazeBounds()
        {
            CheckForErrors();
            return new Bounds(m_Center,
                new Vector3(
                    m_MazeSize.X * m_Scale,
                    m_MazeSize.Y * m_Scale,
                    0f));
        }

        public Vector2 ToGlobalMazePosition(Vector2 _Point)
        {
            CheckForErrors();
            return _Point * m_Scale + m_StartPoint;
        }

        public Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            CheckForErrors();
            return ToGlobalMazePosition(_Point).MinusY(GetMazeCenter().y);
        }
        
        public Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            CheckForErrors();
            return ToGlobalMazePosition(_Point).PlusY(m_Scale * 0.5f).MinusY(GetMazeCenter().y);
        }

        #endregion

        #region nonpublic menhods

        private void SetStartPointAndScale()
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - LeftOffset - RightOffset;
            m_Scale = realBoundsSize / m_MazeSize.X;
            float startX = bounds.min.x + LeftOffset + RightOffset + m_Scale * 0.5f;
            float startY = bounds.min.y + BottomOffset;
            m_StartPoint = new Vector2(startX, startY);
        }

        private void SetCenter()
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - LeftOffset - RightOffset;
            float startY = bounds.min.y + BottomOffset;
            float centerY = startY + realBoundsSize * 0.5f;
            m_Center = new Vector2(bounds.center.x, centerY);
        }

        private void CheckForErrors()
        {
            if (!m_Initialized)
                Dbg.LogError($"Coordinate converter was not initialized.");
            if (m_MazeSize.X <= 0 || m_MazeSize.Y <= 0)
                Dbg.LogError($"Maze size is {m_MazeSize}. Width or height must be greater than zero.");
        }
        
        #endregion
        
    }
}
﻿using DI.Extensions;
using Entities;
using UnityEngine;
using Utils;

namespace Games.RazorMaze
{
    public interface IMazeCoordinateConverter
    {
        bool Initialized();
        V2Int MazeSize { get; set; }
        float Scale { get; }
        void Init(float _LeftOffset, float _RightOffset, float _BottomOffset, float _TopOffset);
        Vector2 GetMazeCenter();
        Bounds GetMazeBounds();
        Vector4 GetScreenOffsets();
        Vector2 ToGlobalMazePosition(Vector2 _Point);
        Vector2 ToLocalMazeItemPosition(Vector2 _Point);
        Vector2 ToLocalCharacterPosition(Vector2 _Point);
    }
    
    public class MazeCoordinateConverter : IMazeCoordinateConverter
    {
        #region constants

        public const float DefaultLeftOffset   = 1f;
        public const float DefaultRightOffset  = 1f;
        public const float DefaultTopOffset    = 10f;
        public const float DefaultBottomOffset = 10f;

        #endregion
        
        #region nonpublic members

        private float m_LeftOffset, m_RightOffset, m_BottomOffset, m_TopOffset;

        private V2Int m_MazeSize;
        private float m_Scale;
        private Vector2 m_Min;
        private Vector2 m_Center;
        private bool m_OffsetsInitialized;
        private bool m_MazeSizeSet;

        #endregion
        
        #region api
        
        public bool Initialized()
        {
            return m_OffsetsInitialized && m_MazeSizeSet;
        }

        public V2Int MazeSize
        {
            get => m_MazeSize;
            set
            {
                m_MazeSize = value;
                m_MazeSizeSet = true;
                if (!CheckForErrors())
                {
                    SetScale();
                    SetMinimumPoint();
                }
            }
        }

        public float Scale
        {
            get
            {
                if (CheckForErrors())
                    return 0f;
                return m_Scale;
            }
        }
        
        public void Init(float _LeftOffset, float _RightOffset, float _BottomOffset, float _TopOffset)
        {
            (m_LeftOffset, m_RightOffset) = (_LeftOffset, _RightOffset);
            (m_BottomOffset, m_TopOffset) = (_BottomOffset, _TopOffset);
            SetCenterPoint();
            m_OffsetsInitialized = true;
        }

        public Vector2 GetMazeCenter()
        {
            return m_Center;
        }

        public Vector4 GetScreenOffsets()
        {
            return new Vector4(
                DefaultLeftOffset,
                DefaultRightOffset, 
                DefaultBottomOffset,
                DefaultTopOffset);
        }
        
        public Bounds GetMazeBounds()
        {
            if (CheckForErrors())
                return default;
            return new Bounds(m_Center,
                new Vector3(
                    m_MazeSize.X * m_Scale,
                    m_MazeSize.Y * m_Scale,
                    0f));
        }

        public Vector2 ToGlobalMazePosition(Vector2 _Point)
        {
            if (CheckForErrors())
                return default;
            return _Point * m_Scale + m_Min;
        }

        public Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            return ToGlobalMazePosition(_Point)
                .PlusX(m_Scale * 0.5f)
                .MinusY(GetMazeCenter().y);
        }
        
        public Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            return ToGlobalMazePosition(_Point)
                .PlusX(m_Scale * 0.5f)
                .PlusY(m_Scale * 0.5f)
                .MinusY(GetMazeCenter().y);
        }

        #endregion

        #region nonpublic menhods

        private void SetScale()
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - m_LeftOffset - m_RightOffset;
            m_Scale = realBoundsSize / m_MazeSize.Y;
        }
        
        private void SetCenterPoint()
        {
            var bounds = GameUtils.GetVisibleBounds();
            float centerX = ((bounds.min.x + m_LeftOffset) + (bounds.max.x - m_RightOffset)) * 0.5f;
            float centerY = ((bounds.min.y + m_BottomOffset) + (bounds.max.y - m_TopOffset)) * 0.5f;
            m_Center = new Vector2(centerX, centerY);
        }

        private void SetMinimumPoint()
        {
            float minX = m_Center.x - m_Scale * m_MazeSize.X * 0.5f;
            float minY = m_Center.y - m_Scale * m_MazeSize.Y * 0.5f;
            m_Min = new Vector2(minX, minY);
        }

        private bool CheckForErrors()
        {
            if (!Initialized())
            {
                Dbg.LogError("Coordinate converter was not initialized.");
                return true;
            }
            if (!m_MazeSizeSet || m_MazeSize.X <= 0 || m_MazeSize.Y <= 0)
            {
                Dbg.LogError($"Maze size is {m_MazeSize}. Width or height must be greater than zero.");
                return true;
            }
            return false;
        }
        
        #endregion
        
    }
}
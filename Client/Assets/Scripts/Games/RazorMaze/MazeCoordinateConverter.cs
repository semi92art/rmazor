using System;
using DI.Extensions;
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
    
    [Serializable]
    public class MazeCoordinateConverter : IMazeCoordinateConverter
    {
        #region constants

        public const float DefaultLeftOffset   = 1f;
        public const float DefaultRightOffset  = 1f;
        public const float DefaultTopOffset    = 10f;
        public const float DefaultBottomOffset = 8f;

        #endregion
        
        #region nonpublic members

        private float m_LeftOffset, m_RightOffset, m_BottomOffset, m_TopOffset;

        private V2Int m_MazeSize;
        private float m_Scale;
        private Vector2 m_Center;
        private bool m_OffsetsInitialized;
        private bool m_MazeSizeSet;

        #endregion

        #region inject

        private ICameraProvider CameraProvider { get; }

        public MazeCoordinateConverter(ICameraProvider _CameraProvider)
        {
            CameraProvider = _CameraProvider;
        }

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
                    SetScale();
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
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider?.MainCamera);
            return new Vector4(
                screenBounds.min.x + m_LeftOffset,
                 screenBounds.max.x - m_RightOffset,
                screenBounds.min.y + m_BottomOffset,
                screenBounds.max.y - m_TopOffset);
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
            return (_Point - m_MazeSize.ToVector2() * 0.5f) * m_Scale;
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
                .MinusY(GetMazeCenter().y)
                .PlusY(m_Scale * 0.5f);
        }

        #endregion

        #region nonpublic menhods

        private void SetCenterPoint()
        {
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider?.MainCamera);
            float centerX = ((bounds.min.x + m_LeftOffset) + (bounds.max.x - m_RightOffset)) * 0.5f;
            float centerY = ((bounds.min.y + m_BottomOffset) + (bounds.max.y - m_TopOffset)) * 0.5f;
            m_Center = new Vector2(centerX, centerY);
        }
        
        private void SetScale()
        {
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider?.MainCamera);
            float sizeX = bounds.size.x - m_LeftOffset - m_RightOffset;
            float sizeY = bounds.size.y - m_BottomOffset - m_TopOffset;
            if ((float) m_MazeSize.X / m_MazeSize.Y > sizeX / sizeY)
                m_Scale = sizeX / m_MazeSize.X;
            else
                m_Scale = sizeY / m_MazeSize.Y;
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
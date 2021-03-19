using System;
using Entities;
using Extensions;
using UnityEngine;
using Utils;
using Zenject;

namespace Games.RazorMaze
{
    public interface ICoordinateConverter
    {
        void Init(int _Size);
        float GetScale();
        Vector2 GetCenter();
        Vector2 ToLocalMazeItemPosition(V2Int _Point);
        Vector2 ToLocalCharacterPosition(V2Int _Point);
    }
    
    public class CoordinateConverter : ICoordinateConverter
    {
        #region nonpublic members

        private float HorizontalOffset => 1f;
        private float TopOffset => 5f;
        private float BottomOffset => 10f;

        private int m_Size;
        private float m_Scale;
        private Vector2 m_StartPoint;
        private Vector2 m_Center;

        #endregion
        
        #region api

        [Inject] public CoordinateConverter() => SetCenter();

        public void Init(int _Size)
        {
            m_Size = _Size;
            CheckForInitialization();
            SetStartPointAndScale(m_Size);
        }

        public float GetScale()
        {
            CheckForInitialization();
            return m_Scale;
        }
        
        public Vector2 ToLocalMazeItemPosition(V2Int _Point)
        {
            CheckForInitialization();
            return (_Point.ToVector2() * m_Scale + m_StartPoint).MinusY(GetCenter().y);
        }
        
        public Vector2 ToLocalCharacterPosition(V2Int _Point)
        {
            CheckForInitialization();
            return (_Point.ToVector2() * m_Scale + m_StartPoint).PlusY(m_Scale * 0.5f).MinusY(GetCenter().y);
        }
        
        public Vector2 GetCenter() => m_Center;

        #endregion

        #region nonpublic menhods

        private void SetStartPointAndScale(int _Size)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - HorizontalOffset * 2f;
            m_Scale = realBoundsSize / _Size;
            float startX = bounds.min.x + HorizontalOffset + m_Scale * 0.5f;
            float startY = bounds.min.y + BottomOffset;
            m_StartPoint = new Vector2(startX, startY);
        }

        private void SetCenter()
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - HorizontalOffset * 2f;
            float startY = bounds.min.y + BottomOffset;
            float centerY = startY + realBoundsSize * 0.5f;
            m_Center = new Vector2(bounds.center.x, centerY);
        }

        private void CheckForInitialization()
        {
            if (m_Size <= 0)
                Dbg.LogError<Exception>("Size must be greater than zero");
        }
        
        #endregion
        
    }
}
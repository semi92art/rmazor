using System;
using Entities;
using Extensions;
using UnityEngine;
using Utils;

namespace Games.RazorMaze
{
    public interface ICoordinateConverter
    {
        void Init(int _Size);
        float GetScale();
        Vector2 GetCenter();
        Vector2 ToWorldPosition(V2Int _Point);
        Vector2 ToLocalPosition(V2Int _Point);
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
        
        public void Init(int _Size)
        {
            m_Size = _Size;
            CheckForInitialization();
            SetStartPointAndScaleAndCenter(m_Size);
        }

        public float GetScale()
        {
            CheckForInitialization();
            return m_Scale;
        }
        
        public Vector2 ToWorldPosition(V2Int _Point)
        {
            CheckForInitialization();
            return _Point.ToVector2() * m_Scale + m_StartPoint;
        }
        
        public Vector2 ToLocalPosition(V2Int _Point)
        {
            return ToWorldPosition(_Point).MinusY(GetCenter().y);
        }
        
        public Vector2 GetCenter()
        {
            CheckForInitialization();
            return m_Center;
        }
        
        #endregion

        #region nonpublic menhods

        private void SetStartPointAndScaleAndCenter(int _Size)
        {
            var bounds = GameUtils.GetVisibleBounds();
            float realBoundsSize = bounds.size.x - HorizontalOffset * 2f;
            m_Scale = realBoundsSize / _Size;
            float itemSize = m_Scale;
            float startX = bounds.min.x + HorizontalOffset + itemSize * 0.5f;
            float startY = bounds.min.y + BottomOffset + itemSize * 0.5f;
            m_StartPoint = new Vector2(startX, startY);
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
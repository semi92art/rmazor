using System;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using RMAZOR.Views.ContainerGetters;
using UnityEngine;
using Zenject;

namespace RMAZOR
{
    public interface IMazeCoordinateConverter
    {
        Func<string, Transform> GetContainer { get; set; }
        bool            InitializedAndMazeSizeSet();
        V2Int           MazeSize { get; set; }
        float           Scale    { get; }
        void            Init();
        Vector2         GetMazeCenter();
        Bounds          GetMazeBounds();
        Vector2         ToGlobalMazeItemPosition(Vector2 _Point);
        Vector2         ToLocalMazeItemPosition(Vector2 _Point);
        Vector2         ToLocalCharacterPosition(Vector2 _Point);
    }
    
    [Serializable]
    public class MazeCoordinateConverter : IMazeCoordinateConverter
    {
        #region serialized fields

        [SerializeField] private Transform mazeItemFake;

        #endregion
        
        #region nonpublic members

        private float m_LeftOffset, m_RightOffset, m_BottomOffset, m_TopOffset;

        private                  V2Int     m_MazeSize;
        private                  float     m_Scale;
        private                  Vector2   m_Center;
        private                  bool      m_Initialized;
        private                  bool      m_MazeSizeSet;
        private                  bool      m_Debug;

        #endregion

        #region inject

        private ViewSettings    ViewSettings   { get; }
        private ICameraProvider CameraProvider { get; }

        [Inject]
        public MazeCoordinateConverter(ViewSettings _ViewSettings, ICameraProvider _CameraProvider)
        {
            ViewSettings = _ViewSettings;
            CameraProvider = _CameraProvider;
        }

        #endregion

        #region constructor

        public MazeCoordinateConverter(ViewSettings _ViewSettings, ICameraProvider _CameraProvider, bool _Debug) 
            : this(_ViewSettings, _CameraProvider)
        {
            m_Debug = _Debug;
        }

        #endregion
        
        #region api

        public Func<string, Transform> GetContainer { get; set; }

        public bool InitializedAndMazeSizeSet()
        {
            return m_Initialized && m_MazeSizeSet;
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
        
        public void Init()
        {
            var vs = ViewSettings;
            (m_LeftOffset, m_RightOffset) = (vs.LeftScreenOffset, vs.RightScreenOffset);
            (m_BottomOffset, m_TopOffset) = (vs.BottomScreenOffset, vs.TopScreenOffset);
            SetCenterPoint();
            m_Initialized = true;
            if (m_Debug)
                return;
            mazeItemFake = CommonUtils.FindOrCreateGameObject("Maze Item Fake", out _).transform;
            mazeItemFake.SetParent(GetContainer(ContainerNames.MazeItems));
        }

        public Vector2 GetMazeCenter()
        {
            return m_Center;
        }

        public Bounds GetMazeBounds()
        {
            if (CheckForErrors())
                return default;
            return new Bounds(m_Center, m_Scale * new Vector2(m_MazeSize.X, m_MazeSize.Y));
        }

        public Vector2 ToGlobalMazeItemPosition(Vector2 _Point)
        {
            mazeItemFake.SetLocalPosXY(ToLocalMazeItemPosition(_Point));
            return mazeItemFake.position;
        }

        public Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point)
                .PlusX(m_Scale * 0.5f);
        }
        
        public Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point)
                .PlusX(m_Scale * 0.5f)
                .PlusY(m_Scale * 0.5f);
        }

        #endregion

        #region nonpublic menhods

        private Vector2 ToLocalMazePosition(Vector2 _Point)
        {
            if (CheckForErrors())
                return default;
            return m_Scale * (_Point - (Vector2)m_MazeSize * 0.5f);
        }

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
            if (!InitializedAndMazeSizeSet())
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
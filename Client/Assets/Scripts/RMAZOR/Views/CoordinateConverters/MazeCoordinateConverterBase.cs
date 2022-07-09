using System;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.CoordinateConverters
{
    public interface ICoordinateConverterRmazorBase
    {
        float                   Scale        { get; }
        Func<string, Transform> GetContainer { set; }
        bool                    IsValid();
        
        void                    Init();
        Vector2                 GetMazeCenter();
        Bounds                  GetMazeBounds();
        Vector2                 ToGlobalMazeItemPosition(Vector2 _Point);
        Vector2                 ToLocalMazeItemPosition(Vector2  _Point);
        Vector2                 ToLocalCharacterPosition(Vector2 _Point);
    }
    
    [Serializable]
    public abstract class CoordinateConverterBase : ICoordinateConverterRmazorBase
    {
        #region serialized fields

        [SerializeField] protected Transform mazeItemFake;

        #endregion

        #region nonpublic members
        
        protected float
            LeftOffset,
            RightOffset,
            BottomOffset,
            TopOffset;
        
        protected bool    Initialized;
        protected bool    MazeDataWasSet;
        protected float   ScaleValue;
        protected Vector2 MazeSize;
        private   Vector2 m_Center;
        protected   bool  Debug;

        #endregion

        #region inject
        
        protected ViewSettings    ViewSettings   { get; set; }
        protected ICameraProvider CameraProvider { get; set; }

        protected CoordinateConverterBase(
            ViewSettings    _ViewSettings,
            ICameraProvider _CameraProvider)
        {
            ViewSettings   = _ViewSettings;
            CameraProvider = _CameraProvider;
        }


        #endregion

        #region api
        
        public Func<string, Transform> GetContainer { protected get;  set; }
        
        public float Scale
        {
            get
            {
                CheckForErrors();
                return ScaleValue;
            }
        }

        public bool IsValid()
        {
            return Initialized && MazeDataWasSet;
        }

        public virtual void Init()
        {
            (float leftScreenOffset, float rightScreenOffset) = RmazorUtils.GetRightAndLeftScreenOffsets();
            var vs = ViewSettings;
            (LeftOffset, RightOffset) = (leftScreenOffset, rightScreenOffset);
            (BottomOffset, TopOffset) = (vs.bottomScreenOffset, vs.topScreenOffset);
            SetCenterPoint();
            Initialized = true;
            if (Debug)
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
            CheckForErrors();
            return new Bounds(m_Center, ScaleValue * new Vector2(MazeSize.x, MazeSize.y));
        }

        public Vector2 ToGlobalMazeItemPosition(Vector2 _Point)
        {
            mazeItemFake.SetLocalPosXY(ToLocalMazeItemPosition(_Point));
            return mazeItemFake.position;
        }

        public Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point)
                .PlusX(ScaleValue * 0.5f);
        }

        public Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point)
                .PlusX(ScaleValue * 0.5f)
                .PlusY(ScaleValue * 0.5f);
        }

        #endregion

        #region nonpublic methods
        
        protected abstract Vector2 ToLocalMazePosition(Vector2 _Point);
        
        protected void SetScale()
        {
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider?.MainCamera);
            float sizeX = bounds.size.x - LeftOffset - RightOffset;
            float sizeY = bounds.size.y - BottomOffset - TopOffset;
            if (MazeSize.x / MazeSize.y > sizeX / sizeY)
                ScaleValue = sizeX / MazeSize.x;
            else
                ScaleValue = sizeY / MazeSize.y;
        }
        
        
        private void SetCenterPoint()
        {
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider?.MainCamera);
            float centerX = ((bounds.min.x + LeftOffset) + (bounds.max.x - RightOffset)) * 0.5f;
            float centerY = ((bounds.min.y + BottomOffset) + (bounds.max.y - TopOffset)) * 0.5f;
            m_Center = new Vector2(centerX, centerY);
        }
        
        protected void CheckForErrors()
        {
            if (!Initialized)
                Dbg.LogError("Coordinate converter was not initialized.");
            if (!MazeDataWasSet || MazeSize.x < MathUtils.Epsilon || MazeSize.y < MathUtils.Epsilon)
                Dbg.LogError("Maze size was not set.");
        }

        #endregion
    }
}
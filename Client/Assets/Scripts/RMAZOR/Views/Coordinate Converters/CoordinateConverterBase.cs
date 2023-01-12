using System;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.Coordinate_Converters
{
    public interface ICoordinateConverterBase
    {
        float                   Scale        { get; }
        Func<string, Transform> GetContainer { set; }
        bool                    IsValid();
        
        void    Init();
        void    SetMazeInfo(MazeInfo _Info);
        Bounds  GetMazeBounds();
        Vector2 ToGlobalMazeItemPosition(Vector2 _Point);
        Vector2 ToLocalMazeItemPosition(Vector2  _Point);
        Vector2 ToLocalCharacterPosition(Vector2 _Point);
    }
    
    [Serializable]
    public abstract class CoordinateConverterBase : ICoordinateConverterBase
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
        protected Vector2 MazeSizeForPositioning = new Vector2(10f, 10f);
        protected Vector2 MazeSizeForScale       = new Vector2(10f, 10f);
        protected Vector2 Center;
        protected bool    IsDebug;

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
            if (IsDebug)
                return;
            mazeItemFake = CommonUtils.FindOrCreateGameObject("Maze Item Fake", out _).transform;
            var container = GetContainer(ContainerNamesMazor.MazeItems);
            mazeItemFake.SetParent(container);
        }

        public abstract void SetMazeInfo(MazeInfo _Info);

        public Bounds GetMazeBounds()
        {
            return new Bounds(
                Center, 
                ScaleValue * new Vector2(MazeSizeForPositioning.x, MazeSizeForPositioning.y));
        }

        public Vector2 ToGlobalMazeItemPosition(Vector2 _Point)
        {
            mazeItemFake.SetLocalPosXY(ToLocalMazeItemPosition(_Point));
            return mazeItemFake.position;
        }

        public abstract Vector2 ToLocalMazeItemPosition(Vector2 _Point);

        public abstract Vector2 ToLocalCharacterPosition(Vector2 _Point);

        #endregion

        #region nonpublic methods
        
        protected abstract Vector2 ToLocalMazePosition(Vector2 _Point);
        
        protected virtual void SetScale()
        {
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider?.Camera);
            float sizeX = bounds.size.x - LeftOffset - RightOffset;
            float sizeY = bounds.size.y - BottomOffset - TopOffset;
            if (MazeSizeForScale.x / MazeSizeForScale.y > sizeX / sizeY)
                ScaleValue = sizeX / MazeSizeForScale.x;
            else
                ScaleValue = sizeY / MazeSizeForScale.y;
        }
        
        protected abstract void SetCenterPoint();

        protected void CheckForErrors()
        {
            if (!Initialized)
                Dbg.LogError("Coordinate converter was not initialized.");
        }

        #endregion
    }
}
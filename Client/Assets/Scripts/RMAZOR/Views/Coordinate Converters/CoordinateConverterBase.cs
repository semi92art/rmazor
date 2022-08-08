using System;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Views.Coordinate_Converters
{
    public interface ICoordinateConverterRmazorBase
    {
        float                   Scale        { get; }
        Func<string, Transform> GetContainer { set; }
        bool                    IsValid();
        
        void                    Init();
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
        protected Vector2 MazeSizeForPositioning;
        protected Vector2 MazeSizeForScale;
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
            mazeItemFake.SetParent(GetContainer(ContainerNames.MazeItems));
        }

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

        public Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point + Vector2.right * .5f);
        }

        public Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point + Vector2.one * .5f);
        }

        #endregion

        #region nonpublic methods
        
        protected abstract Vector2 ToLocalMazePosition(Vector2 _Point);
        
        protected void SetScale()
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
            if (!MazeDataWasSet || MazeSizeForPositioning.x < MathUtils.Epsilon || MazeSizeForPositioning.y < MathUtils.Epsilon)
                Dbg.LogError("Maze size was not set.");
        }

        #endregion
    }
}
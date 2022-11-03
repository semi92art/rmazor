using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.SpawnPools;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.Common.Additional_Background
{
    public interface IViewMazeAdditionalBackgroundDrawer : IInit, IAppear
    {
        void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex);
        void Disable();
    }
    
    public abstract class ViewMazeAdditionalBackgroundDrawerRmazorBase
        : InitBase, 
          IViewMazeAdditionalBackgroundDrawer
    {
        #region constants
        
        private const   int   MasksPoolCount       = 200;
        protected const float BorderRelativeIndent = 0.5f;

        #endregion

        #region nonpublic members
        
        protected readonly BehavioursSpawnPool<Rectangle> TextureRendererMasks = new BehavioursSpawnPool<Rectangle>();
        
        protected float CornerScaleCoefficient => ViewSettings.additionalBackgroundType == 2 ? 4f : 0.5f;
        
        protected Transform Container => ContainersGetter.GetContainer(ContainerNames.MazeItems);

        #endregion

        #region inject
        
        protected ViewSettings         ViewSettings        { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter    ContainersGetter    { get; }
        
        protected ViewMazeAdditionalBackgroundDrawerRmazorBase(
            ViewSettings         _ViewSettings,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter    _ContainersGetter)
        {
            ViewSettings        = _ViewSettings;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
        }

        #endregion

        #region api
        
        public          EAppearingState AppearingState { get; protected set; }
        public abstract void Appear(bool _Appear);

        public abstract void Draw(List<PointsGroupArgs> _Groups, long _LevelIndex);
        
        public virtual void Disable()
        {
            foreach (var mask in TextureRendererMasks
                .Where(_Mask => _Mask.IsNotNull()))
            {
                mask.Color = mask.Color.SetA(0f);
            }
            TextureRendererMasks.DeactivateAll();
        }

        #endregion

        #region nonpublic methods

        protected void InitMasks()
        {
            for (int i = 1; i <= MasksPoolCount; i++)
            {
                var mask = Container.gameObject.AddComponentOnNewChild<Rectangle>(
                    "Additional Texture Mask", 
                    out GameObject _);
                mask.SetBlendMode(ShapesBlendMode.Subtractive)
                    .SetRenderQueue(0)
                    .SetSortingOrder(SortingOrders.AdditionalBackgroundTexture)
                    .SetZTest(CompareFunction.Less)
                    .SetStencilComp(CompareFunction.Greater)
                    .SetStencilOpPass(StencilOp.Replace)
                    .SetColor(new Color(0f, 0f, 0f, 1f / 255f))
                    .SetType(Rectangle.RectangleType.RoundedSolid)
                    .enabled = false;
                TextureRendererMasks.Add(mask);
            }
        }

        protected void DrawMaskForGroup(PointsGroupArgs _Group)
        {
            int minX = _Group.Points.Min(_P => _P.X);
            int minY = _Group.Points.Min(_P => _P.Y);
            int maxX = _Group.Points.Max(_P => _P.X);
            int maxY = _Group.Points.Max(_P => _P.Y);
            float scale = CoordinateConverter.Scale;
            float width = scale * (maxX - minX + 2f * (0.5f + BorderRelativeIndent));
            float height = scale * (maxY - minY + 2f * (0.5f + BorderRelativeIndent));
            var mask = TextureRendererMasks.FirstInactive;
            var centerRaw = new Vector2(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f);
            var center = CoordinateConverter.ToLocalMazeItemPosition(centerRaw);
            mask.SetWidth(width)
                .SetHeight(height)
                .SetStencilRefId(Convert.ToByte(_Group.GroupIndex))
                .SetCornerRadius(ViewSettings.CornerRadius * CornerScaleCoefficient * scale)
                .transform.SetLocalPosXY(center);
            TextureRendererMasks.Activate(mask);
        }

        #endregion
    }
}
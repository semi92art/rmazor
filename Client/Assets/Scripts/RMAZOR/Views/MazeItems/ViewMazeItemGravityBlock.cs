using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemGravityBlock : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlock : ViewMazeItemGravityBlockFree, IViewMazeItemGravityBlock
    {
        #region shapes

        protected override string    ObjectName => "Gravity Block";
        private            Rectangle m_FilledShape;
        
        #endregion
        
        #region inject

        private ViewMazeItemGravityBlock(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter  _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder)
        { }

        #endregion
        
        #region api
        
        public override Component[] Renderers => base.Renderers.Concat(new [] {m_FilledShape}).ToArray();
        
        public override object Clone() => new ViewMazeItemGravityBlock(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);
        

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            base.InitShape();
            m_FilledShape = Object.AddComponentOnNewChild<Rectangle>("Joint", out _)
                .SetSortingOrder(GetSortingOrder())
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetColor(ColorProvider.GetColor(ColorIds.Main));
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            float scale = CoordinateConverter.Scale;
            m_FilledShape.SetSize(scale * 0.9f)
                .SetThickness(ViewSettings.LineThickness * scale)
                .SetCornerRadius(ViewSettings.CornerRadius * scale);
        }

        protected override void InitWallBlockMovingPaths()
        {
            InitWallBlockMovingPathsCore();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Main)
                m_FilledShape.Color = _Color.SetA(0.3f);
            base.OnColorChanged(_ColorId, _Color);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = base.GetAppearSets(_Appear);
            sets.Add(new [] {m_FilledShape}, () => ColorProvider.GetColor(ColorIds.Main).SetA(0.3f));
            return sets;
        }

        #endregion
    }
}
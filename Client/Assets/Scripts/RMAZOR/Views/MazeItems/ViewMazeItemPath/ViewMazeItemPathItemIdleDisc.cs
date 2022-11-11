using System;
using Common;
using Common.Extensions;
using Common.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public class ViewMazeItemPathItemIdleDisc : ViewMazeItemPathItemIdleBase
    {
        #region nonpublic members
        
        private Rectangle m_PathItem;
        
        #endregion

        #region inject
        
        private ViewMazeItemPathItemIdleDisc(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _Transitioner)
            : base(
                _ViewSettings,
                _ColorProvider, 
                _CoordinateConverter,
                _Transitioner) { }

        #endregion

        #region api
        
        public override Component[]     Renderers      => new Component[] { m_PathItem };
        
        public override object Clone() =>
            new ViewMazeItemPathItemIdleDisc(
                ViewSettings,
                ColorProvider,
                CoordinateConverter,
                Transitioner);

        public override void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent)
        {
            if (!ViewSettings.showPathItems)
                return;
            m_PathItem = _Parent.AddComponentOnNewChild<Rectangle>("Path Item", out _)
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetCornerRadiusMode(Rectangle.RectangleCornerRadiusMode.Uniform)
                .SetCornerRadius(CoordinateConverter.Scale * ViewSettings.CornerRadius)
                .SetThickness(CoordinateConverter.Scale * ViewSettings.LineThickness * 0.5f)
                .SetSortingOrder(SortingOrders.Path);
            base.InitShape(_GetProps, _Parent);
        }

        public override void UpdateShape()
        {
            if (!ViewSettings.showPathItems)
                return;
            if (GetProps == null)
                return;
            float scale = CoordinateConverter.Scale;
            if (GetProps().IsMoneyItem)
            {
                m_PathItem.enabled = false;
            }
            else
            {
                m_PathItem
                        .SetWidth(scale * 0.4f)
                        .SetHeight(scale * 0.4f)
                        .SetCornerRadius(scale * ViewSettings.CornerRadius * 2f)
                        .SetThickness(scale * 0.5f * ViewSettings.LineThickness);
            }
            Collect(false);
        }

        public override void Collect(bool _Collect)
        {
            base.Collect(_Collect);
            if (!ViewSettings.showPathItems)
                return;
            var col = ColorProvider.GetColor(ColorIds.PathItem);
            m_PathItem.Color = _Collect ? col.SetA(0f) : col;
        }

        public override void EnableInitializedShapes(bool _Enable)
        {
            if (ViewSettings.showPathItems && m_PathItem.IsNotNull())
                m_PathItem.enabled = _Enable && !GetProps().IsMoneyItem && !GetProps().Blank;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (!ViewSettings.showPathItems)
                return;
            if (_ColorId != ColorIds.PathItem)
                return;
            if (IsCollected)
                return;
            var props = GetProps();
            if (props.Blank || props.IsMoneyItem)
                return;
            m_PathItem.Color = _Color;
        }

        #endregion
    }
}
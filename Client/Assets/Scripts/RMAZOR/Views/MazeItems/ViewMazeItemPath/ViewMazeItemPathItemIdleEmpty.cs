using System;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathItemIdleEmpty : IViewMazeItemPathItemIdle { }
    
    public class ViewMazeItemPathItemIdleEmpty
        : ViewMazeItemPathItemIdleBase, IViewMazeItemPathItemIdleEmpty
    {
        #region inject
        
        private ViewMazeItemPathItemIdleEmpty(
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

        public override Component[] Renderers => new Component[] {};
        
        public override object Clone() =>
            new ViewMazeItemPathItemIdleEmpty(
                ViewSettings,
                ColorProvider,
                CoordinateConverter,
                Transitioner);

        public override void InitShape(Func<ViewMazeItemProps> _GetProps, Transform _Parent)
        {
            if (!ViewSettings.showPathItems)
                return;
            base.InitShape(_GetProps, _Parent);
        }

        public override void UpdateShape()
        {
            if (!ViewSettings.showPathItems)
                return;
            if (GetProps == null)
                return;
            Collect(false);
        }

        public override    void EnableInitializedShapes(bool _Enable)                { }
        protected override void OnColorChanged(int           _ColorId, Color _Color) { }

        #endregion
    }
}
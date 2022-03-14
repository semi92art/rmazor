using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public class ViewMazeItemPathFilled : ViewMazeItemPath
    {
        #region nonpublic members

        private Rectangle m_PathBackground;

        #endregion

        #region inject
        
        private IViewMazeBackgroundTextureController TextureController { get; }
        
        public ViewMazeItemPathFilled(
            ViewSettings                         _ViewSettings,
            IModelGame                           _Model,
            IMazeCoordinateConverter             _CoordinateConverter,
            IContainersGetter                    _ContainersGetter,
            IViewGameTicker                      _GameTicker,
            IViewBetweenLevelTransitioner        _Transitioner,
            IManagersGetter                      _Managers,
            IColorProvider                       _ColorProvider,
            IViewInputCommandsProceeder          _CommandsProceeder,
            IViewMazeBackgroundTextureController _TextureController,
            IViewMazeMoneyItem                   _MoneyItem)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder,
                _MoneyItem)
        {
            TextureController = _TextureController;
        }

        #endregion

        #region api

        public override Component[] Shapes => base.Shapes.Concat(new[] {m_PathBackground}).ToArray();

        public override object Clone() => new ViewMazeItemPathFilled(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder,
            TextureController,
            MoneyItem.Clone() as IViewMazeMoneyItem);
        
        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            base.InitShape();
            InitBackgroundShape();
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            UpdateBackgroundShape();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.Background1)
            {
                m_PathBackground.SetColor(Color.Lerp(
                    _Color,
                ColorProvider.GetColor(ColorIds.Background2),
                0.5f));
            }
            else if (_ColorId == ColorIds.Background2)
            {
                m_PathBackground.SetColor(Color.Lerp(
                    ColorProvider.GetColor(ColorIds.Background1), 
                    _Color,
                    0.5f));
            }
        }

        private void InitBackgroundShape()
        {
            var sh = Object.AddComponentOnNewChild<Rectangle>("Path Item", out _)
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetCornerRadiusMode(Rectangle.RectangleCornerRadiusMode.PerCorner)
                .SetSortingOrder(SortingOrders.PathBackground);
            sh.enabled = false;
            m_PathBackground    = sh;
        }

        private void UpdateBackgroundShape()
        {
            var sh = m_PathBackground;
            float scale = CoordinateConverter.Scale;
            sh.Width = sh.Height = scale * AdditionalScale;
            float radius = ViewSettings.CornerRadius * scale;
            float bottomLeftRadius  = BottomLeftCornerInited  && IsBottomLeftCornerInner  ? radius : 0f;
            float topLeftRadius     = TopLeftCornerInited     && IsTopLeftCornerInner     ? radius : 0f;
            float topRightRadius    = TopRightCornerInited    && IsTopRightCornerInner    ? radius : 0f;
            float bottomRightRadius = BottomRightCornerInited && IsBottomRightCornerInner ? radius : 0f;
            sh.CornerRadii = new Vector4(
                bottomLeftRadius, 
                topLeftRadius, 
                topRightRadius, 
                bottomRightRadius);
        }

        protected override void EnableInitializedShapes(bool _Enable)
        {
            base.EnableInitializedShapes(_Enable);
            if (m_PathBackground.IsNotNull())
                m_PathBackground.enabled = _Enable;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            TextureController.GetBackgroundColors(
                out Color current1, out Color current2,
                out _, out _, out _, out _);
            var averageCol = Color.Lerp(current1,current2,0.5f);
            var result = base.GetAppearSets(_Appear);
            result.Add(new [] {m_PathBackground}, () => averageCol);
            return result;
        }

        protected override Color MainColorToBorderColor(Color _Color)
        {
            return _Color;
        }

        #endregion
    }
}
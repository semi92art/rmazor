﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
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
        private Rectangle m_PassedPathBackground;

        #endregion

        #region inject
        
        public ViewMazeItemPathFilled(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewMazeMoneyItem            _MoneyItem)
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
                _MoneyItem) { }

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
            switch (_ColorId)
            {
                case ColorIds.PathBackground: m_PathBackground.SetColor(_Color.SetA(ViewSettings.filledPathAlpha)); break;
                case ColorIds.PathFill:
                    var col = _Color;
                    if (_Color == ColorProvider.GetColor(ColorIds.Main))
                    {
                        Color.RGBToHSV(col, out float h, out float s, out float v);
                        v += v < 0.5f ? 0.3f : -0.3f;
                        col = Color.HSVToRGB(h, s, v);
                    }
                    m_PassedPathBackground.SetColor(col);
                    break;
            }
        }

        public override void Collect(bool _Collect, bool _OnStart = false)
        {
            if (_OnStart && (IsAnyBlockWithSamePosition(EMazeItemType.Portal) 
                             || IsAnyBlockWithSamePosition(EMazeItemType.ShredingerBlock)))
            {
                if (!_Collect)
                    return;
                m_PassedPathBackground.enabled = false;
                base.Collect(true, true);
                return;
            }
            if (_Collect)
                Fill();
            else
                m_PassedPathBackground.enabled = false;
            base.Collect(_Collect, _OnStart);
        }

        private void Fill()
        {
            GetCornerRadii(out float bottomLeft, out float topLeft, 
            out float topRight, out float bottomRight);
            m_PassedPathBackground.enabled = true;
            float fullSize = CoordinateConverter.Scale;
            float size = fullSize * (1f + 2f * AdditionalScale);
            m_PassedPathBackground
                .SetWidth(size)
                .SetHeight(size)
                .SetCornerRadii(new Vector4(bottomLeft, topLeft, topRight, bottomRight));
        }

        private void InitBackgroundShape()
        {
            var sh = Object.AddComponentOnNewChild<Rectangle>("Path Item Background", out _)
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetCornerRadiusMode(Rectangle.RectangleCornerRadiusMode.PerCorner)
                .SetSortingOrder(SortingOrders.PathBackground);
            var sh2 = Object.AddComponentOnNewChild<Rectangle>("Filled Path Item Background", out _)
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetCornerRadiusMode(Rectangle.RectangleCornerRadiusMode.PerCorner)
                .SetSortingOrder(SortingOrders.PathBackground + 1);
            sh.enabled = false;
            sh2.enabled = false;
            m_PathBackground = sh;
            m_PassedPathBackground = sh2;
        }

        private void UpdateBackgroundShape()
        {
            m_PassedPathBackground.enabled = false;
            var sh = m_PathBackground;
            float scale = CoordinateConverter.Scale;
            sh.Width = sh.Height = scale * (1f + AdditionalScale);
            GetCornerRadii(out float bottomLeft,
                out float topLeft, 
                out float topRight, 
                out float bottomRight);
            sh.CornerRadii = new Vector4(
                bottomLeft, 
                topLeft, 
                topRight, 
                bottomRight);
        }

        protected override void EnableInitializedShapes(bool _Enable)
        {
            base.EnableInitializedShapes(_Enable);
            if (m_PathBackground.IsNotNull())
                m_PathBackground.enabled = _Enable;
            if (m_PassedPathBackground.IsNotNull())
                m_PassedPathBackground.enabled = _Enable;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = ColorProvider.GetColor(ColorIds.PathBackground).SetA(ViewSettings.filledPathAlpha);
            var result = base.GetAppearSets(_Appear);
            result.Add(new [] {m_PathBackground}, () => col);
            return result;
        }

        protected override Color GetBorderColor()
        {
            return ColorProvider.GetColor(ColorIds.Main);
        }

        private void GetCornerRadii(
            out float _BottomLeft,
            out float _TopLeft,
            out float _TopRight, 
            out float _BottomRight)
        {
            float radius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
            _BottomLeft  = BottomLeftCornerInited  && IsBottomLeftCornerInner  ? radius : 0f;
            _TopLeft     = TopLeftCornerInited     && IsTopLeftCornerInner     ? radius : 0f;
            _TopRight    = TopRightCornerInited    && IsTopRightCornerInner    ? radius : 0f;
            _BottomRight = BottomRightCornerInited && IsBottomRightCornerInner ? radius : 0f;
        }
        
        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (!_Appear) 
                return;
            if (m_PassedPathBackground.IsNotNull())
                m_PassedPathBackground.enabled = false;
        }

        #endregion
    }
}
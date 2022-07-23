using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemPathFilled : IViewMazeItemPath
    {
        void HighlightPathItem(float _Delay);
    }
    
    public class ViewMazeItemPathFilledWithAdditionalBorders : 
        ViewMazeItemPathWithAdditionalBorders, 
        IViewMazeItemPathFilled
    {
        #region nonpublic members

        private Rectangle m_PathBackground;
        private Rectangle m_PassedPathBackground;

        private bool        m_HighlightPathItemBackground           = true;
        private IEnumerator m_HighlightPathItemCoroutine;
        private float       m_HighlightPathItemCoefficient;
        private float       m_HighlightPathItemCoefficientRaw;
        private int         m_HighlightPathItemCoroutineNumber;

        #endregion

        #region inject

        private ViewMazeItemPathFilledWithAdditionalBorders(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewMazeMoneyItem          _MoneyItem)
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

        public override Component[] Renderers => base.Renderers.Concat(new[] {m_PathBackground}).ToArray();

        public override object Clone() => new ViewMazeItemPathFilledWithAdditionalBorders(
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
        
        public override void Collect(bool _Collect, bool _OnStart = false)
        {
            if (_OnStart && (IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType.Portal) 
                             || IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType.ShredingerBlock)))
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

        public void HighlightPathItem(float _Delay)
        {
            m_HighlightPathItemCoroutine = CharacterMoveHighlightCoroutine(_Delay);
            Cor.Run(m_HighlightPathItemCoroutine);
        }

        public override void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            HighlightPathItemBackground();
            base.UpdateTick();
        }

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
                case ColorIds.PathBackground:
                    m_PathBackground.SetColor(_Color.SetA(ViewSettings.filledPathAlpha));
                    break;
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
        
        private void Fill()
        {
            GetCornerRadii(out float bottomLeft, out float topLeft, 
            out float topRight, out float bottomRight);
            m_PassedPathBackground.enabled = true;
            float fullSize = CoordinateConverter.Scale;
            float size = fullSize * (1f + 2f * AdditionalScale);
            var radii = new Vector4(bottomLeft, topLeft, topRight, bottomRight);
            m_PassedPathBackground.SetCornerRadii(radii);
            FillCore(ViewSettings.animatePathFill, size, radii);
        }

        private void FillCore(bool _Animate, float _Size, Vector4 _CornerRadii)
        {
            if (!_Animate)
            {
                m_PassedPathBackground
                    .SetWidth(_Size)
                    .SetHeight(_Size)
                    .SetCornerRadii(_CornerRadii);
                return;
            }
            float defaultCorderRadius = _Size * 0.5f;
            Cor.Run(Cor.Lerp(
                GameTicker,
                ViewSettings.animatePathFillTime,
                0f,
                1f,
                _P =>
                {
                    float size = Mathf.Lerp(0f, _Size, _P);
                    float bottomLeft  = Mathf.Lerp(defaultCorderRadius, _CornerRadii.x, _P);
                    float topLeft     = Mathf.Lerp(defaultCorderRadius, _CornerRadii.y, _P);
                    float topRight    = Mathf.Lerp(defaultCorderRadius, _CornerRadii.z, _P);
                    float bottomRight = Mathf.Lerp(defaultCorderRadius, _CornerRadii.w, _P);
                    var radii = new Vector4(bottomLeft, topLeft, topRight, bottomRight);
                    m_PassedPathBackground.SetWidth(size).SetHeight(size).SetCornerRadii(radii);
                },
                _ProgressFormula: _P => _P));
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

        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (!_Appear) 
                return;
            if (m_PassedPathBackground.IsNotNull())
                m_PassedPathBackground.enabled = false;
        }

        private void HighlightPathItemBackground()
        {
            if (!MoneyItem.IsCollected)
                return;
            if (!m_HighlightPathItemBackground)
                return;
            var col = ColorProvider
                .GetColor(ColorIds.PathFill)
                .SetA(ViewSettings.filledPathAlpha);
            Color.RGBToHSV(col, out float h1, out float s1, out float v1);
            Color.RGBToHSV(m_PassedPathBackground.Color, out _, out _, out float v2);
            const float highlightAmplitude = 0.3f;
            if (v1 < 1f - highlightAmplitude)
            {
                v1 += m_HighlightPathItemCoefficient * highlightAmplitude;
                if (v2 > v1 && m_HighlightPathItemCoefficientRaw < 0.5f)
                    return;
            }
            else
            {
                v1 -= m_HighlightPathItemCoefficient * highlightAmplitude;
                if (v2 < v1 && m_HighlightPathItemCoefficientRaw < 0.5f)
                    return;
            }
            col = Color.HSVToRGB(h1, s1, v1);
            m_PassedPathBackground.SetColor(col);
        }
        
        private IEnumerator CharacterMoveHighlightCoroutine(float _Delay)
        {
            yield return Cor.Delay(_Delay, GameTicker);
            yield return Cor.Delay(0.08f, GameTicker);
            int num = ++m_HighlightPathItemCoroutineNumber;
            yield return Cor.Lerp(
                GameTicker,
                ViewSettings.pathItemCharMoveHighlightTime,
                _OnProgress: _P =>
                {
                    m_HighlightPathItemCoefficient = _P < 0.5f ? 2f * _P : 2f * (1f - _P);
                    m_HighlightPathItemCoefficientRaw = _P;
                },
                _OnFinishEx: (_Broken, _Progress) =>
                {
                    if (!_Broken || !Model.PathItemsProceeder.AllPathsProceeded) 
                        return;
                    m_HighlightPathItemBackground = false;
                    m_HighlightPathItemCoefficient = 0f;
                    m_PassedPathBackground.Color = ColorProvider
                        .GetColor(ColorIds.PathFill)
                        .SetA(ViewSettings.filledPathAlpha);
                },
                _BreakPredicate: () => num != m_HighlightPathItemCoroutineNumber 
                                       || Model.PathItemsProceeder.AllPathsProceeded);
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathRmazor : IViewMazeItemPath
    {
        bool HighlightEnabled { get; set; }
        void HighlightPathItem(float _Delay);
    }
    
    public class ViewMazeItemPathRmazor : 
        ViewMazeItemPath, 
        IViewMazeItemPathRmazor
    {
        #region nonpublic members

        private Rectangle m_PathBackground;
        private Rectangle m_PassedPathBackground;

        private IEnumerator m_HighlightPathItemCoroutine;
        private float       m_HighlightPathItemCoefficient;
        private float       m_HighlightPathItemCoefficientRaw;
        private int         m_HighlightPathItemCoroutineNumber;
        private bool        m_ExtraBordersInitialized;

        #endregion

        #region inject

        private IViewMazeItemPathExtraBorders1 ExtraBorders1 { get; }
        private IViewMazeItemPathExtraBorders2 ExtraBorders2 { get; }
        private IViewMazeItemPathExtraBorders3 ExtraBorders3 { get; }
        
        private ViewMazeItemPathRmazor(
            ViewSettings                   _ViewSettings,
            IModelGame                     _Model,
            ICoordinateConverter           _CoordinateConverter,
            IContainersGetter              _ContainersGetter,
            IViewGameTicker                _GameTicker,
            IRendererAppearTransitioner    _Transitioner,
            IManagersGetter                _Managers,
            IColorProvider                 _ColorProvider,
            IViewInputCommandsProceeder    _CommandsProceeder,
            IViewMazeMoneyItem             _MoneyItem,
            IViewMazeItemsPathInformer     _Informer,
            IViewMazeItemPathExtraBorders1 _ExtraBorders1,
            IViewMazeItemPathExtraBorders2 _ExtraBorders2, 
            IViewMazeItemPathExtraBorders3 _ExtraBorders3)
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
                _MoneyItem,
                _Informer)
        {
            ExtraBorders1 = _ExtraBorders1;
            ExtraBorders2 = _ExtraBorders2;
            ExtraBorders3 = _ExtraBorders3;
        }

        #endregion

        #region api

        public override Component[] Renderers
        {
            get
            {
                var renderers = base.Renderers
                    .Concat(new[] {m_PathBackground});
                return renderers
                    .Concat(GetCurrentExtraBorders().Renderers)
                    .ToArray();
            }
        }

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                ActivateCurrentExtraBorders(value);
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void Init()
        {
            // FIXME при раскоменчивании возникает ошибка
            // if (Initialized)
            //     return;
            Informer.GetProps = () => Props;
            InitExtraBorders();
            base.Init();
        }

        public override object Clone() => new ViewMazeItemPathRmazor(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder,
            MoneyItem.Clone() as IViewMazeMoneyItem,
            Informer,
            ExtraBorders1.Clone() as IViewMazeItemPathExtraBorders1, 
            ExtraBorders2.Clone() as IViewMazeItemPathExtraBorders2,
            ExtraBorders3.Clone() as IViewMazeItemPathExtraBorders3);

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart 
                    when _Args.PreviousStage == ELevelStage.Paused 
                         && _Args.PrePreviousStage == ELevelStage.CharacterKilled:
                {
                    HighlightEnabled = true;
                }
                    break;
            }
        }

        public override void Collect(bool _Collect, bool _OnStart = false)
        {
            bool isPortalOrShredingerWithSamePos =
                Informer.IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType.Portal) 
                || Informer.IsAnyBlockOfConcreteTypeWithSamePosition(EMazeItemType.ShredingerBlock);
            if (_OnStart && isPortalOrShredingerWithSamePos)
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

        public bool HighlightEnabled { get; set; }

        public void HighlightPathItem(float _Delay)
        {
            Cor.Stop(m_HighlightPathItemCoroutine);
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

        private void InitExtraBorders()
        {
            var extraBordersArray = new IViewMazeItemPathExtraBorders[]
            {
                ExtraBorders1, ExtraBorders2, ExtraBorders3
            };
            foreach (var extraBorders in extraBordersArray)
            {
                extraBorders.GetParent      = () => Object;
                extraBorders.GetProps       = () => Props;
                extraBorders.GetBorderColor = GetBorderColor;
                extraBorders.Init();
            }
        }

        protected override void InitShape()
        {
            base.InitShape();
            InitBackgroundShape();
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            UpdateBackgroundShape();
            HighlightEnabled = true;
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
                    var col = GetPathFillColorCorrected(_Color);
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
            GetCurrentExtraBorders().EnableInitializedShapes(_Enable);
            if (m_PathBackground.IsNotNull())
                m_PathBackground.enabled = _Enable;
            if (m_PassedPathBackground.IsNotNull())
                m_PassedPathBackground.enabled = _Enable;
        }
        
        protected override void HighlightBordersAndCorners()
        {
            base.HighlightBordersAndCorners();
            GetCurrentExtraBorders().HighlightBordersAndCorners();
        }

        protected override void DrawBorders()
        {
            base.DrawBorders();
            GetCurrentExtraBorders().DrawBorders();
        }

        protected override void AdjustBorders()
        {
            base.AdjustBorders();
            GetCurrentExtraBorders().AdjustBorders();
        }

        protected override void AdjustBordersOnCornerInitialization(bool _Right, bool _Up, bool _Inner)
        {
            base.AdjustBordersOnCornerInitialization(_Right, _Up, _Inner);
            GetCurrentExtraBorders().AdjustBordersOnCornerInitialization(_Right, _Up, _Inner);
        }
        
        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (!_Appear) 
                return;
            if (m_PassedPathBackground.IsNotNull())
                m_PassedPathBackground.enabled = false;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = ColorProvider.GetColor(ColorIds.PathBackground).SetA(ViewSettings.filledPathAlpha);
            var result = base.GetAppearSets(_Appear);
            result.Add(new [] {m_PathBackground}, () => col);
            return result;
        }

        private void HighlightPathItemBackground()
        {
            if (!GetCanHighlightCommonPredicate()
                || m_HighlightPathItemCoroutine == null)
            {
                return;
            }
            var col = ColorProvider.GetColor(ColorIds.PathFill);
            col = GetPathFillColorCorrected(col);
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
            yield return Cor.Delay(0.07f, GameTicker);
            m_PassedPathBackground.SetSortingOrder(SortingOrders.PathBackground + 2);
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
                    var col = ColorProvider.GetColor(ColorIds.PathFill);
                    col = GetPathFillColorCorrected(col);
                    m_PassedPathBackground
                        .SetColor(col)
                        .SetSortingOrder(SortingOrders.PathBackground + 1);
                    if (!_Broken)
                        m_HighlightPathItemCoroutine = null;
                },
                _BreakPredicate: () => num != m_HighlightPathItemCoroutineNumber
                                       || !GetCanHighlightCommonPredicate());
        }

        private Color GetPathFillColorCorrected(Color _ColorRaw)
        {
            if (_ColorRaw != ColorProvider.GetColor(ColorIds.Main))
                return _ColorRaw;
            var col = _ColorRaw;
            Color.RGBToHSV(col, out float h, out float s, out float v);
            col = Color.HSVToRGB(h, s, v);
            return col;
        }

        private bool GetCanHighlightCommonPredicate()
        {
            return ActivatedInSpawnPool
                   && MoneyItem.IsCollected
                   && HighlightEnabled;
        }

        private void ActivateCurrentExtraBorders(bool _Activate)
        {
            ExtraBorders1.Activated = false;
            ExtraBorders2.Activated = false;
            ExtraBorders3.Activated = false;
            GetCurrentExtraBorders().Activated = _Activate;
        }

        private IViewMazeItemPathExtraBorders GetCurrentExtraBorders()
        {
            int levelGroupIndex = RmazorUtils.GetGroupIndex(Model.LevelStaging.LevelIndex);
            int extraBordersIndex = (levelGroupIndex - 1) % 3;
            IViewMazeItemPathExtraBorders extraBorders = extraBordersIndex switch
            {
                0 => ExtraBorders1,
                1 => ExtraBorders2,
                2 => ExtraBorders3,
                _ => throw new SwitchExpressionException(extraBordersIndex)
            };
            return extraBorders;
        }

        #endregion
    }
}
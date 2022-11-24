using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.Additional
{
    public class ViewTurretBodySquare : ViewTurretBodyBase, IUpdateTick
    {
        #region constants

        private const float ProjectileContainerRadius = 0.4f;
        
        #endregion
        
        #region nonpublic members
        
        private Rectangle m_Body;
        private Disc      m_HolderBorder;

        #endregion

        #region inject

        private ViewTurretBodySquare(
            IModelGame                  _Model,
            IViewGameTicker             _GameTicker,
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _Transitioner)
            : base(
                _Model,
                _GameTicker,
                _ViewSettings,
                _ColorProvider, 
                _CoordinateConverter, 
                _Transitioner) { }

        #endregion

        #region api
        
        public override object Clone()
        {
            return new ViewTurretBodySquare(
                Model, 
                GameTicker, 
                ViewSettings, 
                ColorProvider,
                CoordinateConverter,
                Transitioner);
        }

        public override bool Activated
        {
            get => base.Activated;
            set
            {
                if (!value)
                {
                    m_Body.enabled = false;
                    m_HolderBorder.enabled = false;
                }
                base.Activated = value;
            }
        }

        public override void Init()
        {
            GameTicker.Register(this);
            base.Init();
        }

        public override void OpenBarrel(bool _Open, bool _Instantly = false, bool _Forced = false)
        {
            Cor.Run(OpenBarrelCore(_Open, _Instantly, _Forced));
        }

        public override void HighlightBarrel(bool _Open, bool _Instantly = false, bool _Forced = false)
        {
            Cor.Run(HighlightBarrelCore(_Open, _Instantly, _Forced));
        }
        
        public void UpdateTick()
        {
            m_HolderBorder.DashOffset = MathUtils.ClampInverse(
                m_HolderBorder.DashOffset += 2f * GameTicker.DeltaTime,
                0f, 10f);
        }
        
        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Main: 
                    m_Body.SetColor(_Color);
                    m_HolderBorder.SetColor(_Color);
                    break;
            }
            base.OnColorChanged(_ColorId, _Color);
        }

        protected override void InitShape()
        {
            base.InitShape();
            int sortingOrder = SortingOrders.GetBlockSortingOrder(EMazeItemType.Turret);
            m_Body = Container.AddComponentOnNewChild<Rectangle>("Turret Body", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.Main))
                .SetSortingOrder(sortingOrder)
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetDashed(true)
                .SetDashSize(0f)
                .SetMatchDashSpacingToDashSize(false)
                .SetDashSize(0.5f)
                .SetDashSnap(DashSnapping.Tiling)
                .SetDashType(DashType.Rounded);
            m_HolderBorder = Container.AddComponentOnNewChild<Disc>("Border", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.MazeItem1))
                .SetSortingOrder(sortingOrder + 1)
                .SetType(DiscType.Ring)
                .SetDashed(true)
                .SetDashType(DashType.Rounded)
                .SetDashSize(2f);
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            float scale = CoordinateConverter.Scale;
            m_Body.SetHeight(scale * (1f + ViewSettings.LineThickness))
                .SetWidth(scale * (1f + ViewSettings.LineThickness))
                .SetThickness(scale * ViewSettings.LineThickness)
                .SetCornerRadius(scale * ViewSettings.LineThickness);
            m_HolderBorder.SetRadius(scale * ProjectileContainerRadius * 0.9f)
                .SetThickness(ViewSettings.LineThickness * scale * 0.5f);
        }

        private IEnumerator OpenBarrelCore(bool _Open, bool _Instantly, bool _Forced)
        {
            float offsetOpened, spacingOpened, offsetClosed, spacingClosed;
            (offsetOpened, spacingOpened) = GetBarrelRectOffsetAndSpacing(true);
            (offsetClosed, spacingClosed) = GetBarrelRectOffsetAndSpacing(false);
            float offsetFrom = _Open  ? offsetClosed : offsetOpened;
            float offsetTo   = !_Open ? offsetClosed : offsetOpened;
            float spacingFrom   = _Open  ? spacingClosed   : spacingOpened;
            float spacingTo     = !_Open ? spacingClosed   : spacingOpened;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.SetDashOffset(offsetTo).SetDashSpacing(spacingTo);
                yield break;
            }
            yield return Cor.Lerp(
                GameTicker,
                0.1f,
                _OnProgress: _P =>
                {
                    float offset = Mathf.Lerp(offsetFrom, offsetTo, _P);
                    float spacing = Mathf.Lerp(spacingFrom, spacingTo, _P);
                    m_Body.SetDashOffset(offset).SetDashSpacing(spacing);
                },
                _BreakPredicate: () =>
                {
                    if (_Forced)
                        return false;
                    return Model.LevelStaging.LevelStage == ELevelStage.Finished;
                });
        }
        
        private IEnumerator HighlightBarrelCore(bool _Open, bool _Instantly, bool _Forced)
        {
            Color DefCol() => ColorProvider.GetColor(ColorIds.Main);
            var highlightCol = ColorProvider.GetColor(ColorIds.MazeItem1);
            Color StartCol() => _Open ? DefCol() : highlightCol;
            Color EndCol()   => !_Open ? DefCol() : highlightCol;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.Color = EndCol();
                yield break;
            }
            yield return Cor.Lerp(
                GameTicker,
                0.1f,
                _OnProgress: _P =>
                {
                    m_Body.Color = Color.Lerp(StartCol(), EndCol(), _P);
                },
                _BreakPredicate: () => AppearingState != EAppearingState.Appeared
                                       || Model.LevelStaging.LevelStage == ELevelStage.Finished);
        }
        
        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
                m_Body.enabled = true;
            base.OnAppearStart(_Appear);
        }
        
        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                m_Body.enabled = false;
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var dict = base.GetAppearSets(_Appear);
            var col = ColorProvider.GetColor(ColorIds.Main);
            dict.Add(new Component[] {m_Body, m_HolderBorder}, () => col);
            return dict;
        }
        
        protected override Vector4 GetBackgroundCornerRadii()
        {
            var pos = Props.Position;
            float radius = CoordinateConverter.Scale * ViewSettings.LineThickness;
            float bottomLeftR  = IsPathItem(pos + V2Int.Down + V2Int.Left)  ? 0f : radius;
            float topLeftR     = IsPathItem(pos + V2Int.Up + V2Int.Left)    ? 0f : radius;
            float topRightR    = IsPathItem(pos + V2Int.Up + V2Int.Right)   ? 0f : radius;
            float bottomRightR = IsPathItem(pos + V2Int.Down + V2Int.Right) ? 0f : radius;
            return new Vector4(bottomLeftR, topLeftR, topRightR, bottomRightR);
        }

        private Tuple<float, float> GetBarrelRectOffsetAndSpacing(bool _Opened)
        {
            var dir = Props.Directions.First();
            float offset = default;
            float spacing = _Opened ? 0.15f : 0.05f;
            if (dir == V2Int.Left)       offset = 0.5f;
            else if (dir == V2Int.Right) offset = 0f;
            else if (dir == V2Int.Up)    offset =  0.25f;
            else if (dir == V2Int.Down)  offset =  0.75f;
            offset += spacing * 0.5f;
            return new Tuple<float, float>(offset, spacing);
        }

        #endregion
    }
}
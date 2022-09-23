using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Enums;
using Shapes;
using UnityEngine;
using Common.Extensions;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.MazeItems.Additional
{
    public class ViewTurretBodyCircle : ViewTurretBodyBase, IUpdateTick
    {
        #region constants

        private const float ProjectileContainerRadius = 0.4f;
        
        #endregion
        
        #region nonpublic members

        private Disc m_Body;
        private Disc m_HolderBorder;
        
        #endregion

        #region inject

        private ViewTurretBodyCircle(
            IModelGame                  _Model,
            IViewGameTicker             _GameTicker,
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            ICoordinateConverter  _CoordinateConverter,
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
            return new ViewTurretBodyCircle(
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
                case ColorIds.Main: m_Body.Color = _Color; break;
            }
            base.OnColorChanged(_ColorId, _Color);
        }

        protected override void InitShape()
        {
            base.InitShape();
            int sortingOrder = GetSortingOrder();
            m_Body = Container.AddComponentOnNewChild<Disc>("Turret Body", out _)
                .SetColor(ColorProvider.GetColor(ColorIds.Main))
                .SetSortingOrder(sortingOrder)
                .SetType(DiscType.Arc)
                .SetArcEndCaps(ArcEndCap.Round);
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
            m_Body.SetRadius(scale * 0.5f)
                .SetThickness(scale * ViewSettings.LineThickness);
            m_HolderBorder.SetRadius(scale * ProjectileContainerRadius * 0.9f)
                .SetThickness(ViewSettings.LineThickness * scale * 0.5f);
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
            dict.Add(new [] {m_Body, m_HolderBorder}, () => col);
            return dict;
        }

        private IEnumerator OpenBarrelCore(bool _Open, bool _Instantly, bool _Forced)
        {
            float openedStart, openedEnd, closedStart, closedEnd;
            (openedStart, openedEnd) = GetBarrelDiscAngles(true);
            (closedStart, closedEnd) = GetBarrelDiscAngles(false);
            float startFrom = _Open  ? closedStart : openedStart;
            float startTo   = !_Open ? closedStart : openedStart;
            float endFrom   = _Open  ? closedEnd   : openedEnd;
            float endTo     = !_Open ? closedEnd   : openedEnd;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.AngRadiansStart = startTo;
                m_Body.AngRadiansEnd = endTo;
                yield break;
            }
            yield return Cor.Lerp(
                GameTicker,
                0.1f,
                _OnProgress: _P =>
                {
                    m_Body.AngRadiansStart = Mathf.Lerp(startFrom, startTo, _P);
                    m_Body.AngRadiansEnd = Mathf.Lerp(endFrom, endTo, _P);
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
        
        protected override Vector4 GetBackgroundCornerRadii()
        {
            var pos = Props.Position;
            float radius = CoordinateConverter.Scale * 0.5f;
            float bottomLeftR  = IsPathItem(pos + V2Int.Down + V2Int.Left)  ? 0f : radius;
            float topLeftR     = IsPathItem(pos + V2Int.Up + V2Int.Left)    ? 0f : radius;
            float topRightR    = IsPathItem(pos + V2Int.Up + V2Int.Right)   ? 0f : radius;
            float bottomRightR = IsPathItem(pos + V2Int.Down + V2Int.Right) ? 0f : radius;
            return new Vector4(bottomLeftR, topLeftR, topRightR, bottomRightR);
        }
        
        private Tuple<float, float> GetBarrelDiscAngles(bool _Opened)
        {
            var dir = Props.Directions.First();
            float angleStart = 0f;
            float angleEnd = 0f;
            if (dir == V2Int.Left)
            {
                angleStart = _Opened ? -135f : -160f;
                angleEnd = _Opened ? 135f : 160f;
            }
            else if (dir == V2Int.Right)
            {
                angleStart = _Opened ? 45f : 20f;
                angleEnd = _Opened ? 315f : 340f;
            }
            else if (dir == V2Int.Up)
            {
                angleStart = _Opened ? -225f : -250f;
                angleEnd = _Opened ? 45f : 70f;
            }
            else if (dir == V2Int.Down)
            {
                angleStart = _Opened ? -45f : -70f;
                angleEnd = _Opened ? 225f : 250f;
            }
            return new Tuple<float, float>(angleStart * Mathf.Deg2Rad, angleEnd * Mathf.Deg2Rad);
        }

        #endregion
    }
}
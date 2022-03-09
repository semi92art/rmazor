using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemPath : IViewMazeItem
    {
        bool        Collected          { get; set; }
        UnityAction MoneyItemCollected { get; set; } 
    }
    
    public class ViewMazeItemPath : ViewMazeItemBase, IViewMazeItemPath, IUpdateTick
    {
        #region constants

        protected const float AdditionalScale = 1.01f;

        #endregion
        
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsCollectMoneyItem => 
            new AudioClipArgs("collect_point", EAudioClipType.GameSound);
        protected override string ObjectName => "Path Block";
        
        private Rectangle      m_PathItem;
        private Line           m_LeftBorder,       m_RightBorder,       m_BottomBorder,  m_TopBorder;
        private Disc           m_BottomLeftCorner, m_BottomRightCorner, m_TopLeftCorner, m_TopRightCorner;
        
        private bool 
            m_LeftBorderInited, 
            m_RightBorderInited,
            m_BottomBorderInited,
            m_TopBorderInited;
        protected bool 
            BottomLeftCornerInited,
            BottomRightCornerInited,
            TopLeftCornerInited, 
            TopRightCornerInited;
        protected bool
            IsBottomLeftCornerInner,
            IsBottomRightCornerInner,
            IsTopLeftCornerInner,
            IsTopRightCornerInner;
        private bool m_BottomLeftCornerIsOuterAndNearTrapIncreasing;
        private bool m_BottomRightCornerIsOuterAndNearTrapIncreasing;
        private bool m_TopLeftCornerIsOuterAndNearTrapIncreasing;
        private bool m_TopRightCornerIsOuterAndNearTrapIncreasing;
        private bool m_IsLeftBorderInverseOffset;
        private bool m_IsRightBorderInverseOffset;
        private bool m_IsBottomBorderInverseOffset;
        private bool m_IsTopBorderInverseOffset;
        private float m_DashedOffset;
        
        #endregion
        
        #region inject
        
        protected IViewMazeMoneyItem MoneyItem { get; }
        
        // ReSharper disable once MemberCanBeProtected.Global
        public ViewMazeItemPath(
            ViewSettings                      _ViewSettings,
            IModelGame                        _Model,
            IMazeCoordinateConverter          _CoordinateConverter,
            IContainersGetter                 _ContainersGetter,
            IViewGameTicker                   _GameTicker,
            IViewBetweenLevelTransitioner     _Transitioner,
            IManagersGetter                   _Managers,
            IColorProvider                    _ColorProvider,
            IViewInputCommandsProceeder       _CommandsProceeder,
            IViewMazeMoneyItem                _MoneyItem)
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
        {
            MoneyItem = _MoneyItem;
        }
        
        #endregion

        #region api

        public override Component[] Shapes => new Component[]
        {
            m_PathItem,
            m_LeftBorder, m_RightBorder, m_BottomBorder, m_TopBorder,
            m_BottomLeftCorner, m_BottomRightCorner, m_TopLeftCorner, m_TopRightCorner
        }.Concat(MoneyItem.Renderers).ToArray();
        
        public override object Clone() => new ViewMazeItemPath(
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

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                EnableInitializedShapes(false);
            }
        }
        
        public bool Collected
        {
            get => MoneyItem.IsCollected;
            set
            {
                MoneyItem.IsCollected = value;
                Collect(value);
            }
        }

        public UnityAction MoneyItemCollected { get; set; }

        public override void Init(ViewMazeItemProps _Props)
        {
            if (!Initialized)
                Managers.AudioManager.InitClip(AudioClipArgsCollectMoneyItem);
            base.Init(_Props);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            MoneyItem.OnLevelStageChanged(_Args);
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            const float maxOffset = 10f;
            float dOffset = GameTicker.DeltaTime * 3f;
            m_DashedOffset += dOffset;
            m_DashedOffset = MathUtils.ClampInverse(m_DashedOffset, 0, maxOffset);
            if (m_LeftBorderInited && m_LeftBorder.Dashed)
            {
                float offset = !m_IsLeftBorderInverseOffset ? -m_DashedOffset : m_DashedOffset + 0.5f;
                m_LeftBorder.DashOffset = offset;
            }
            if (m_RightBorderInited && m_RightBorder.Dashed)
            {
                float offset = !m_IsRightBorderInverseOffset ? m_DashedOffset : -1f * (m_DashedOffset + 0.5f);
                m_RightBorder.DashOffset = offset;
            }
            if (m_BottomBorderInited && m_BottomBorder.Dashed)
            {
                float offset = !m_IsBottomBorderInverseOffset ? m_DashedOffset : -1f * (m_DashedOffset + 0.5f);
                m_BottomBorder.DashOffset = offset;
            }
            if (m_TopBorderInited && m_TopBorder.Dashed)
            {
                float offset = !m_IsTopBorderInverseOffset ? -m_DashedOffset : m_DashedOffset + 0.5f;
                m_TopBorder.DashOffset = offset;
            }
        }

        #endregion
        
        #region nonpublic methods

        private void Collect(bool _Collect)
        {
            MoneyItem.Collect(_Collect);
            var col = ColorProvider.GetColor(ColorIds.Main);
            m_PathItem.Color = _Collect ? col.SetA(0f) : col;
        }

        protected override void InitShape()
        {
            MoneyItem.Collected += () => MoneyItemCollected?.Invoke();
            var sh              = Object.AddComponentOnNewChild<Rectangle>("Path Item", out _);
            sh.Type             = Rectangle.RectangleType.RoundedBorder;
            sh.CornerRadiusMode = Rectangle.RectangleCornerRadiusMode.Uniform;
            sh.CornerRadius     = CoordinateConverter.Scale * ViewSettings.CornerRadius;
            sh.Thickness        = CoordinateConverter.Scale * ViewSettings.LineWidth * 0.5f;
            sh.Color            = ColorProvider.GetColor(ColorIds.Main);
            sh.SortingOrder     = SortingOrders.Path;
            sh.enabled          = false;
            m_PathItem          = sh;
        }

        protected override void UpdateShape()
        {
            if (Props.IsMoneyItem)
            {
                if (MoneyItem.Renderers.Any(_R => _R.IsNull()))
                    MoneyItem.Init(Object.transform);
                MoneyItem.Active = true;
                m_PathItem.enabled = false;
            }
            else
            {
                MoneyItem.UpdateShape();
                MoneyItem.Active = false;
                m_PathItem.Width = m_PathItem.Height = CoordinateConverter.Scale * 0.4f;
                m_PathItem.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale * 2f;
            }
            if (Model.PathItemsProceeder.PathProceeds[Props.Position])
                Collect(true);
            SetBordersAndCorners();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.Main) 
                return;
            if (BottomLeftCornerInited && !m_BottomLeftCornerIsOuterAndNearTrapIncreasing)
                m_BottomLeftCorner.Color = _Color;
            if (BottomRightCornerInited && !m_BottomRightCornerIsOuterAndNearTrapIncreasing)
                m_BottomRightCorner.Color = _Color;
            if (TopLeftCornerInited && !m_TopLeftCornerIsOuterAndNearTrapIncreasing)
                m_TopLeftCorner.Color = _Color;
            if (TopRightCornerInited && !m_TopRightCornerIsOuterAndNearTrapIncreasing)
                m_TopRightCorner.Color = _Color;
            
            if (!Collected || Props.Blank || Props.IsMoneyItem)
                m_PathItem.Color = _Color;

            var borderCol = MainColorToBorderColor(_Color);
            if (m_LeftBorderInited)
                m_LeftBorder.Color = borderCol;
            if (m_RightBorderInited)
                m_RightBorder.Color = borderCol;
            if (m_BottomBorderInited)
                m_BottomBorder.Color = borderCol;
            if (m_TopBorderInited)
                m_TopBorder.Color = borderCol;
            if (BottomLeftCornerInited && m_BottomLeftCornerIsOuterAndNearTrapIncreasing)
                m_BottomLeftCorner.Color = borderCol;
            if (BottomRightCornerInited && m_BottomRightCornerIsOuterAndNearTrapIncreasing)
                m_BottomRightCorner.Color = borderCol;
            if (TopLeftCornerInited && m_TopLeftCornerIsOuterAndNearTrapIncreasing)
                m_TopLeftCorner.Color = borderCol;
            if (TopRightCornerInited && m_TopRightCornerIsOuterAndNearTrapIncreasing)
                m_TopRightCorner.Color = borderCol;
        }

        private void SetBordersAndCorners()
        {
            ClearBordersAndCorners();
            InitBorders();
            InitInnerCorners();
            InitOuterCorners();
            AdjustBorders();
            EnableInitializedShapes(false);
        }

        private void ClearBordersAndCorners()
        {
            if (m_LeftBorderInited)   (m_LeftBorder.enabled, m_LeftBorder.DashOffset)     = (false, 0f);
            if (m_RightBorderInited)  (m_RightBorder.enabled, m_RightBorder.DashOffset)   = (false, 0f);
            if (m_BottomBorderInited) (m_BottomBorder.enabled, m_BottomBorder.DashOffset) = (false, 0f);
            if (m_TopBorderInited)    (m_TopBorder.enabled, m_TopBorder.DashOffset)       = (false, 0f);
            
            if (BottomLeftCornerInited)  m_BottomLeftCorner .enabled = false;
            if (BottomRightCornerInited) m_BottomRightCorner.enabled = false;
            if (TopLeftCornerInited)     m_TopLeftCorner    .enabled = false;
            if (TopRightCornerInited)    m_TopRightCorner   .enabled = false;
            
            m_LeftBorderInited = m_RightBorderInited = m_BottomBorderInited = m_TopBorderInited = false;
            BottomLeftCornerInited = BottomRightCornerInited = TopLeftCornerInited = TopRightCornerInited = false;
            m_IsLeftBorderInverseOffset = m_IsRightBorderInverseOffset =
                m_IsBottomBorderInverseOffset = m_IsTopBorderInverseOffset = false;
        }

        private void InitBorders()
        {
            bool MustInitBorder(V2Int _Pos) => !TurretExist(_Pos) && !PathExist(_Pos);
            var pos = Props.Position;
            if (MustInitBorder(pos + V2Int.Left))
                InitBorder(EMazeMoveDirection.Left);
            if (MustInitBorder(pos + V2Int.Right))
                InitBorder(EMazeMoveDirection.Right);
            if (MustInitBorder(pos + V2Int.Up))
                InitBorder(EMazeMoveDirection.Up);
            if (MustInitBorder(pos + V2Int.Down))
                InitBorder(EMazeMoveDirection.Down);
        }

        private void InitInnerCorners()
        {
            var pos = Props.Position;
            bool initLeftBottomCorner = !PathExist(pos + V2Int.Left)
                                      && !PathExist(pos + V2Int.Down)
                                      || SpringboardExist(pos) 
                                      && SpringboardDirection(pos) == V2Int.Right + V2Int.Up;
            if (initLeftBottomCorner)
                InitCorner(false, false, true);
            bool initLeftTopCorner = !PathExist(pos + V2Int.Left)
                                     && !PathExist(pos + V2Int.Up)
                                     || SpringboardExist(pos)
                                     && SpringboardDirection(pos) == V2Int.Right + V2Int.Down;
            if (initLeftTopCorner)
                InitCorner(false, true, true);
            bool initRightBottomCorner = !PathExist(pos + V2Int.Right)
                                         && !PathExist(pos + V2Int.Down)
                                         || SpringboardExist(pos)
                                         && SpringboardDirection(pos) == V2Int.Left + V2Int.Up;
            if (initRightBottomCorner)
                InitCorner(true, false, true);
            bool initRightTopCorner = !PathExist(pos + V2Int.Right)
                                      && !PathExist(pos + V2Int.Up)
                                      || SpringboardExist(pos)
                                      && SpringboardDirection(pos) == V2Int.Left + V2Int.Down;
            if (initRightTopCorner)
                InitCorner(true, true, true);
        }

        private void InitOuterCorners()
        {
            var pos = Props.Position;
            if (MustInitOuterCorner(pos, V2Int.Down, V2Int.Left))
                InitCorner(false, false, false);
            if (MustInitOuterCorner(pos, V2Int.Down, V2Int.Right))
                InitCorner(true, false, false);
            if (MustInitOuterCorner(pos, V2Int.Up, V2Int.Left))
                InitCorner(false, true, false);
            if (MustInitOuterCorner(pos, V2Int.Up, V2Int.Right))
                InitCorner(true, true, false);
        }

        private bool MustInitOuterCorner(V2Int _Position, V2Int _Dir1, V2Int _Dir2)
        {
            var pos = _Position;
            bool result = PathExist(pos + _Dir1)
                    && PathExist(pos + _Dir2) 
                    && !PathExist(pos + _Dir1 + _Dir2)
                    && !TurretExist(pos + _Dir1 + _Dir2);
            if (result)
                return true;
            result = TurretExist(pos + _Dir1) 
                    && PathExist(pos + _Dir2) 
                    && !PathExist(pos + _Dir1 + _Dir2)
                    && !TurretExist(pos + _Dir1 + _Dir2);
            if (result)
                return true;
            result = TurretExist(pos + _Dir2) 
                    && PathExist(pos + _Dir1) 
                    && !PathExist(pos + _Dir1 + _Dir2)
                    && !TurretExist(pos + _Dir1 + _Dir2);
            return result;
        }

        private void AdjustBorders()
        {
            bool adjStart, adjEnd;
            var pos = Props.Position;
            AdjustBorder(pos, V2Int.Left, m_LeftBorderInited, ref m_LeftBorder, out adjStart, out adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsLeftBorderInverseOffset = true;
                (m_LeftBorder.Start, m_LeftBorder.End) = (m_LeftBorder.End, m_LeftBorder.Start);
            }
            AdjustBorder(pos, V2Int.Right, m_RightBorderInited, ref m_RightBorder, out adjStart, out adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsRightBorderInverseOffset = true;
                (m_RightBorder.Start, m_RightBorder.End) = (m_RightBorder.End, m_RightBorder.Start);
            }
            AdjustBorder(pos, V2Int.Down, m_BottomBorderInited, ref m_BottomBorder, out adjStart, out adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsBottomBorderInverseOffset = true;
                (m_BottomBorder.Start, m_BottomBorder.End) = (m_BottomBorder.End, m_BottomBorder.Start);
            }
            AdjustBorder(pos, V2Int.Up, m_TopBorderInited, ref m_TopBorder, out adjStart, out adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsTopBorderInverseOffset = true;
                (m_TopBorder.Start, m_TopBorder.End) = (m_TopBorder.End, m_TopBorder.Start);
            }
        }

        private void AdjustBorder(
            V2Int _Position,
            V2Int _Direction,
            bool _BorderInitialized,
            ref Line _Border,
            out bool _AdjustStart,
            out bool _AdjustEnd)
        {
            var dir1 = _Direction == V2Int.Left || _Direction == V2Int.Right ? V2Int.Down : V2Int.Left;
            var dir2 = _Direction == V2Int.Left || _Direction == V2Int.Right ? V2Int.Up : V2Int.Right;
            _AdjustStart = false;
            _AdjustEnd = false;
            if (PathExist(_Position + _Direction) || !_BorderInitialized) 
                return;
            var (start, end, _) = GetBorderPointsAndDashed(_Direction, true, true);
            if (PathExist(_Position + _Direction + dir1) || TurretExist(_Position + _Direction + dir1))
                (_AdjustStart, _Border.Start) = (true, start);
            if (PathExist(_Position + _Direction + dir2) || TurretExist(_Position + _Direction + dir2))
                (_AdjustEnd, _Border.End) = (true, end);
            if (!PathExist(_Position + dir1))
                (_AdjustStart, _Border.Start) = (true, start);
            _Border.DashSize = 4f * Vector3.Distance(_Border.Start, _Border.End) / CoordinateConverter.Scale;
        }

        protected virtual void EnableInitializedShapes(bool _Enable)
        {
            if (m_PathItem.IsNotNull() && !Props.IsMoneyItem)   
                m_PathItem.enabled = _Enable;
            if (MoneyItem.Renderers.All(_R => _R.IsNotNull()) && Props.IsMoneyItem)
                MoneyItem.Active = _Enable;
            if (m_LeftBorderInited)        m_LeftBorder.enabled        = _Enable;
            if (m_RightBorderInited)       m_RightBorder.enabled       = _Enable;
            if (m_BottomBorderInited)      m_BottomBorder.enabled      = _Enable;
            if (m_TopBorderInited)         m_TopBorder.enabled         = _Enable;
            
            if (BottomLeftCornerInited)  m_BottomLeftCorner.enabled  = _Enable;
            if (BottomRightCornerInited) m_BottomRightCorner.enabled = _Enable;
            if (TopLeftCornerInited)     m_TopLeftCorner.enabled     = _Enable;
            if (TopRightCornerInited)    m_TopRightCorner.enabled    = _Enable;
        }
        
        private void InitBorder(EMazeMoveDirection _Side)
        {
            Line border;
            switch (_Side)
            {
                case EMazeMoveDirection.Up:
                    border = m_TopBorder; break;
                case EMazeMoveDirection.Right:border = m_RightBorder; break;
                case EMazeMoveDirection.Down: border = m_BottomBorder; break;
                case EMazeMoveDirection.Left: border = m_LeftBorder; break;
                default: throw new SwitchCaseNotImplementedException(_Side);
            }
            if (border.IsNull())
                border = Object.AddComponentOnNewChild<Line>("Border", out _);
            border.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            border.EndCaps = LineEndCap.None;
            border.Color = MainColorToBorderColor(ColorProvider.GetColor(ColorIds.Main));
            border.SortingOrder = SortingOrders.PathLine;
            (border.Start, border.End, border.Dashed) = GetBorderPointsAndDashed(_Side, false, false);
            border.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            border.enabled = false;
            border.DashSpace = DashSpace.FixedCount;
            border.DashSnap = DashSnapping.Off;
            border.DashType = DashType.Rounded;
            switch (_Side)
            {
                case EMazeMoveDirection.Up: m_TopBorder = border; m_TopBorderInited = true; break;
                case EMazeMoveDirection.Right: m_RightBorder = border; m_RightBorderInited = true; break;
                case EMazeMoveDirection.Down: m_BottomBorder = border; m_BottomBorderInited = true; break;
                case EMazeMoveDirection.Left: m_LeftBorder = border; m_LeftBorderInited = true; break;
                default: throw new SwitchCaseNotImplementedException(_Side);
            }
        }

        private void InitCorner(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            Disc corner = null;
            if (!_Right && !_Up && m_BottomLeftCorner.IsNotNull())
                corner = m_BottomLeftCorner;
            else if (_Right && !_Up && m_BottomRightCorner.IsNotNull())
                corner = m_BottomRightCorner;
            else if (!_Right && _Up && m_TopLeftCorner.IsNotNull())
                corner = m_TopLeftCorner;
            else if (_Right && _Up && m_TopRightCorner.IsNotNull())
                corner = m_TopRightCorner;
            if (corner.IsNull())
                corner = Object.AddComponentOnNewChild<Disc>("Corner", out _);
            // ReSharper disable once PossibleNullReferenceException
            corner.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position; //-V3080
            corner.transform.PlusLocalPosXY(GetCornerCenter(_Right, _Up, _Inner));
            corner.Type = DiscType.Arc;
            corner.ArcEndCaps = ArcEndCap.None;
            corner.Radius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
            corner.Thickness = ViewSettings.CornerWidth * CoordinateConverter.Scale;
            var angles = GetCornerAngles(_Right, _Up, _Inner);
            corner.AngRadiansStart = Mathf.Deg2Rad * angles.x;
            corner.AngRadiansEnd = Mathf.Deg2Rad * angles.y;
            bool isOuterAndNearTrapIncreasing = IsCornerOuterAndNearTrapIncreasing(_Right, _Up, _Inner);
            var col = ColorProvider.GetColor(ColorIds.Main);
            corner.Color = col;
            corner.SortingOrder = SortingOrders.PathJoint;
            corner.enabled = false;
            
            if (!_Right && !_Up)
            {
                m_BottomLeftCorner = corner;
                m_BottomLeftCornerIsOuterAndNearTrapIncreasing = isOuterAndNearTrapIncreasing;
                BottomLeftCornerInited = true;
                IsBottomLeftCornerInner = _Inner;
                if (!_Inner) return;
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item1;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item1;
            }
            else if (_Right && !_Up)
            {
                m_BottomRightCorner = corner;
                m_BottomRightCornerIsOuterAndNearTrapIncreasing = isOuterAndNearTrapIncreasing;
                BottomRightCornerInited = true;
                IsBottomRightCornerInner = _Inner;
                if (!_Inner) return;
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Down, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item1;
            }
            else if (!_Right)
            {
                m_TopLeftCorner = corner;
                m_TopLeftCornerIsOuterAndNearTrapIncreasing = isOuterAndNearTrapIncreasing;
                TopLeftCornerInited = true;
                IsTopLeftCornerInner = _Inner;
                if (!_Inner) return;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Left, true, true).Item2;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.Start = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item1;
            }
            else
            {
                m_TopRightCorner = corner;
                m_TopRightCornerIsOuterAndNearTrapIncreasing = isOuterAndNearTrapIncreasing;
                TopRightCornerInited = true;
                IsTopRightCornerInner = _Inner;
                if (!_Inner) return;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Up, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.End = GetBorderPointsAndDashed(EMazeMoveDirection.Right, true, true).Item2;
            }
        }

        private Tuple<Vector2, Vector2, bool> GetBorderPointsAndDashed(V2Int _Direction, bool _StartLimit,
            bool _EndLimit)
        {
            EMazeMoveDirection side = default;
            if (_Direction == V2Int.Left)
                side = EMazeMoveDirection.Left;
            else if (_Direction == V2Int.Right)
                side = EMazeMoveDirection.Right;
            else if (_Direction == V2Int.Up)
                side = EMazeMoveDirection.Up;
            else if (_Direction == V2Int.Down)
                side = EMazeMoveDirection.Down;
            return GetBorderPointsAndDashed(side, _StartLimit, _EndLimit);
        }

        private Tuple<Vector2, Vector2, bool> GetBorderPointsAndDashed(EMazeMoveDirection _Side, bool _StartLimit, bool _EndLimit)
        {
            Vector2 start, end;
            Vector2 pos = Props.Position;
            float cr = ViewSettings.CornerRadius;
            switch (_Side)
            {
                case EMazeMoveDirection.Up:
                    start = pos + (Vector2.up + Vector2.left * AdditionalScale) * 0.5f + (_StartLimit ? Vector2.right * cr : Vector2.zero);
                    end = pos + (Vector2.up + Vector2.right * AdditionalScale) * 0.5f + (_EndLimit ? Vector2.left * cr : Vector2.zero);
                    break;
                case EMazeMoveDirection.Right:
                    start = pos + (Vector2.right + Vector2.down * AdditionalScale) * 0.5f + (_StartLimit ? Vector2.up * cr : Vector2.zero);
                    end = pos + (Vector2.right + Vector2.up * AdditionalScale) * 0.5f + (_EndLimit ? Vector2.down * cr : Vector2.zero);
                    break;
                case EMazeMoveDirection.Down:
                    start = pos + (Vector2.down + Vector2.left * AdditionalScale) * 0.5f + (_StartLimit ? Vector2.right * cr : Vector2.zero);
                    end = pos + (Vector2.down + Vector2.right * AdditionalScale) * 0.5f + (_EndLimit ? Vector2.left * cr : Vector2.zero);
                    break;
                case EMazeMoveDirection.Left:
                    start = pos + (Vector2.left + Vector2.down * AdditionalScale) * 0.5f + (_StartLimit ? Vector2.up * cr : Vector2.zero);
                    end = pos + (Vector2.left + Vector2.up * AdditionalScale) * 0.5f + (_EndLimit ? Vector2.down * cr : Vector2.zero);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);

            var dir = RazorMazeUtils.GetDirectionVector(_Side, MazeOrientation.North);
            bool dashed = Model.GetAllProceedInfos().Any(_Item =>
                _Item.CurrentPosition == Props.Position + dir &&
                (_Item.Type == EMazeItemType.TrapReact && _Item.Direction == -dir
                || _Item.Type == EMazeItemType.TrapIncreasing));
            
            return new Tuple<Vector2, Vector2, bool>(start, end, dashed);
        }

        private Vector2 GetCornerCenter(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            Vector2 pos = Props.Position;
            float cr = ViewSettings.CornerRadius;
            float xIndent = (_Right ? 1f : -1f) * (_Inner ? -1f : 1f);
            float yIndent = (_Up ? 1f : -1f) * (_Inner ? -1f : 1f);
            var crVec = cr * new Vector2(xIndent, yIndent);
            var center = pos
                         + ((_Right ? Vector2.right : Vector2.left)
                            + (_Up ? Vector2.up : Vector2.down)) * 0.5f + crVec;
            return CoordinateConverter.ToLocalMazeItemPosition(center);
        }

        private static Vector2 GetCornerAngles(bool _Right, bool _Up, bool _Inner)
        {
            if (_Right)
            {
                if (_Up)
                    return _Inner ? new Vector2(0, 90) : new Vector2(180, 270);
                return _Inner ? new Vector2(270, 360) : new Vector2(90, 180);
            }
            if (_Up)
                return _Inner ? new Vector2(90, 180) : new Vector2(270, 360);
            return _Inner ? new Vector2(180, 270) : new Vector2(0, 90);
        }

        private bool IsCornerOuterAndNearTrapIncreasing(bool _Right, bool _Up, bool _Inner)
        {
            if (_Inner)
                return false;
            if (!_Right && !_Up)
                return TrapIncreasingExist(Props.Position + V2Int.Down + V2Int.Left);
            if (_Right && !_Up)
                return TrapIncreasingExist(Props.Position + V2Int.Down + V2Int.Right);
            if (!_Right && _Up)
                return TrapIncreasingExist(Props.Position + V2Int.Up + V2Int.Left);
            if (_Right && _Up)
                return TrapIncreasingExist(Props.Position + V2Int.Up + V2Int.Right);
            return false;
        }

        private bool PathExist(V2Int _Position) => Model.PathItemsProceeder.PathProceeds.Keys.Contains(_Position);

        private bool TurretExist(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.Turret) != null;
        }

        private bool SpringboardExist(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.Springboard) != null;
        }

        private V2Int SpringboardDirection(V2Int _Position)
        {
            var info = GetItemInfo(_Position, EMazeItemType.Springboard);
            if (info != null) 
                return info.Direction;
            Dbg.LogError("Info cannot be null");
            return default;
        }

        private bool TrapIncreasingExist(V2Int _Position)
        {
            return GetItemInfo(_Position, EMazeItemType.TrapIncreasing) != null;
        }

        private IMazeItemProceedInfo GetItemInfo(V2Int _Position, EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos()?
                .FirstOrDefault(_Item => _Item.CurrentPosition == _Position
                                && _Item.Type == _Type); 
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (!_Appear) 
                return;
            EnableInitializedShapes(true);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var borderSets = GetBorderAppearSets();
            var cornerSets = GetCornerAppearSets();
            var result = borderSets.ConcatWithDictionary(cornerSets);
            if (_Appear && !Props.IsStartNode || !_Appear && !Collected)
            {
                if (Props.IsMoneyItem)
                    result.Add(MoneyItem.Renderers, () => ColorProvider.GetColor(ColorIds.MoneyItem));
                else if (!Collected)
                    result.Add(new [] {m_PathItem}, () => ColorProvider.GetColor(ColorIds.Main));
            }
            else
            {
                if (Props.IsMoneyItem)
                    MoneyItem.Active = false;
                m_PathItem.enabled = false;
            }
            return result;
        }
        
        private Dictionary<IEnumerable<Component>, Func<Color>> GetBorderAppearSets()
        {
            var borders = new List<Component>();
            if (m_BottomBorderInited)
                borders.Add(m_BottomBorder);
            if (m_TopBorderInited)
                borders.Add(m_TopBorder);
            if (m_LeftBorderInited)
                borders.Add(m_LeftBorder);
            if (m_RightBorderInited)
                borders.Add(m_RightBorder);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {borders, () => MainColorToBorderColor(ColorProvider.GetColor(ColorIds.Main))}
            };
            return sets;
        }
        
        private Dictionary<IEnumerable<Component>, Func<Color>> GetCornerAppearSets()
        {
            var setsRaw = new Dictionary<Color, List<Component>>();
            var defCornerCol = ColorProvider.GetColor(ColorIds.Main);
            var altCornerCol = ColorProvider.GetColor(ColorIds.Main);
            setsRaw.Add(defCornerCol, new List<Component>());
            if (altCornerCol != defCornerCol)
                setsRaw.Add(altCornerCol, new List<Component>());
            if (BottomLeftCornerInited)
                setsRaw[m_BottomLeftCornerIsOuterAndNearTrapIncreasing ? altCornerCol : defCornerCol]
                    .Add(m_BottomLeftCorner);
            if (BottomRightCornerInited)
                setsRaw[m_BottomRightCornerIsOuterAndNearTrapIncreasing ? altCornerCol : defCornerCol]
                    .Add(m_BottomRightCorner);
            if (TopLeftCornerInited)
                setsRaw[m_TopLeftCornerIsOuterAndNearTrapIncreasing ? altCornerCol : defCornerCol]
                    .Add(m_TopLeftCorner);
            if (TopRightCornerInited)
                setsRaw[m_TopRightCornerIsOuterAndNearTrapIncreasing ? altCornerCol : defCornerCol]
                    .Add(m_TopRightCorner);
            var sets = setsRaw.
                ToDictionary(
                    _Kvp => (IEnumerable<Component>)_Kvp.Value,
                    _Kvp => new Func<Color>(() => _Kvp.Key));
            return sets;
        }

        protected virtual Color MainColorToBorderColor(Color _Color)
        {
            return _Color.SetA(0.5f);
        }

        #endregion
    }
}
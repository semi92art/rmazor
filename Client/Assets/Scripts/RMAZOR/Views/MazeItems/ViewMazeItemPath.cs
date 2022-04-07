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
        event UnityAction MoneyItemCollected;
        void              Collect(bool _Collect, bool _OnStart = false);
    }
    
    public class ViewMazeItemPath : ViewMazeItemBase, IViewMazeItemPath, IUpdateTick
    {
        #region constants

        protected const float AdditionalScale = 0.01f;

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
        private bool m_IsLeftBorderInverseOffset;
        private bool m_IsRightBorderInverseOffset;
        private bool m_IsBottomBorderInverseOffset;
        private bool m_IsTopBorderInverseOffset;
        private float m_DashedOffset;
        
        private bool Collected => !Props.Blank && MoneyItem.IsCollected;
        
        #endregion
        
        #region inject
        
        protected IViewMazeMoneyItem MoneyItem { get; }
        
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
        
        public event UnityAction MoneyItemCollected;

        public override void Init(ViewMazeItemProps _Props)
        {
            if (!Initialized)
                Managers.AudioManager.InitClip(AudioClipArgsCollectMoneyItem);
            MoneyItem.Collected -= MoneyItemCollected;
            MoneyItem.Collected += MoneyItemCollected;
            base.Init(_Props);
        }
        
        public virtual void Collect(bool _Collect, bool _OnStart = false)
        {
            if (Props.Blank)
                return;
            MoneyItem.IsCollected = _Collect;
            MoneyItem.Collect(_Collect);
            var col = ColorProvider.GetColor(GetPathItemColorId());
            m_PathItem.Color = _Collect ? col.SetA(0f) : col;
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

            // var col = ColorProvider.GetColor(ColorIds.Main);
            // Color.RGBToHSV(col, out float h, out float s, out float v);
            // const float amplitude = 0.2f;
            // if (s > 1f - amplitude)
            //     s = 1f - amplitude;
            // else if (s < amplitude)
            //     s = amplitude;
            // s += amplitude * Mathf.Cos(Time.time * 3.0f);
            // s = Mathf.Clamp01(s);
            // var newCol = Color.HSVToRGB(h, s, v);
            // if (m_LeftBorderInited)
            //     m_LeftBorder.SetColor(newCol);
            // if (m_RightBorderInited)
            //     m_RightBorder.SetColor(newCol);
            // if (m_BottomBorderInited)
            //     m_BottomBorder.SetColor(newCol);
            // if (m_TopBorderInited)
            //     m_TopBorder.SetColor(newCol);
            // if (BottomLeftCornerInited)
            //     m_BottomLeftCorner.SetColor(newCol);
            // if (TopLeftCornerInited)
            //     m_TopLeftCorner.SetColor(newCol);
            // if (TopRightCornerInited)
            //     m_TopRightCorner.SetColor(newCol);
            // if (BottomRightCornerInited)
            //     m_BottomRightCorner.SetColor(newCol);
        }

        #endregion
        
        #region nonpublic methods



        protected override void InitShape()
        {
            m_PathItem = Object.AddComponentOnNewChild<Rectangle>("Path Item", out _)
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetCornerRadiusMode(Rectangle.RectangleCornerRadiusMode.Uniform)
                .SetCornerRadius(CoordinateConverter.Scale * ViewSettings.CornerRadius)
                .SetThickness(CoordinateConverter.Scale * ViewSettings.LineWidth * 0.5f)
                // .SetColor(ColorProvider.GetColor(GetPathItemColorId()))
                .SetSortingOrder(SortingOrders.Path);
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
            if (Props.Blank)
                Collect(true, true);
            if (ViewSettings.collectStartPathItemOnLevelLoaded 
                && Model.PathItemsProceeder.PathProceeds[Props.Position])
                Collect(true, true);
            if (IsAnyBlockWithSamePosition(EMazeItemType.ShredingerBlock)
                || IsAnyBlockWithSamePosition(EMazeItemType.Portal))
            {
                Collect(true, true);
            }
            SetBordersAndCorners();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == GetPathItemColorId())
            {
                if (!Collected || Props.Blank || Props.IsMoneyItem)
                    m_PathItem.Color = _Color;
            }
            if (_ColorId != ColorIds.Main) 
                return;
            if (BottomLeftCornerInited)  m_BottomLeftCorner.Color  = _Color;
            if (BottomRightCornerInited) m_BottomRightCorner.Color = _Color;
            if (TopLeftCornerInited)     m_TopLeftCorner.Color     = _Color;
            if (TopRightCornerInited)    m_TopRightCorner.Color    = _Color;
            var borderCol = GetBorderColor();
            if (m_LeftBorderInited)      m_LeftBorder.Color   = borderCol;
            if (m_RightBorderInited)     m_RightBorder.Color  = borderCol;
            if (m_BottomBorderInited)    m_BottomBorder.Color = borderCol;
            if (m_TopBorderInited)       m_TopBorder.Color    = borderCol;
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
            var pos = Props.Position;
            AdjustBorder(pos, V2Int.Left, m_LeftBorderInited,
                ref m_LeftBorder, out bool adjStart, out bool adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsLeftBorderInverseOffset = true;
                (m_LeftBorder.Start, m_LeftBorder.End) = (m_LeftBorder.End, m_LeftBorder.Start);
            }
            AdjustBorder(pos, V2Int.Right, m_RightBorderInited,
                ref m_RightBorder, out adjStart, out adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsRightBorderInverseOffset = true;
                (m_RightBorder.Start, m_RightBorder.End) = (m_RightBorder.End, m_RightBorder.Start);
            }
            AdjustBorder(pos, V2Int.Down, m_BottomBorderInited, 
                ref m_BottomBorder, out adjStart, out adjEnd);
            if (adjStart && !adjEnd)
            {
                m_IsBottomBorderInverseOffset = true;
                (m_BottomBorder.Start, m_BottomBorder.End) = (m_BottomBorder.End, m_BottomBorder.Start);
            }
            AdjustBorder(pos, V2Int.Up, m_TopBorderInited, 
                ref m_TopBorder, out adjStart, out adjEnd);
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
            if (m_PathItem.IsNotNull() && !Props.IsMoneyItem && !Props.Blank)   
                m_PathItem.enabled = _Enable;
            if (MoneyItem.Renderers.All(_R => _R.IsNotNull()) && Props.IsMoneyItem && !Props.Blank)
                MoneyItem.Active = _Enable;
            if (m_LeftBorderInited)      m_LeftBorder.enabled        = _Enable;
            if (m_RightBorderInited)     m_RightBorder.enabled       = _Enable;
            if (m_BottomBorderInited)    m_BottomBorder.enabled      = _Enable;
            if (m_TopBorderInited)       m_TopBorder.enabled         = _Enable;
            
            if (BottomLeftCornerInited)  m_BottomLeftCorner.enabled  = _Enable;
            if (BottomRightCornerInited) m_BottomRightCorner.enabled = _Enable;
            if (TopLeftCornerInited)     m_TopLeftCorner.enabled     = _Enable;
            if (TopRightCornerInited)    m_TopRightCorner.enabled    = _Enable;
        }
        
        private void InitBorder(EMazeMoveDirection _Side)
        {
            Line border = _Side switch
            {
                EMazeMoveDirection.Up    => m_TopBorder,
                EMazeMoveDirection.Right => m_RightBorder,
                EMazeMoveDirection.Down  => m_BottomBorder,
                EMazeMoveDirection.Left  => m_LeftBorder,
                _                        => throw new SwitchCaseNotImplementedException(_Side)
            };
            if (border.IsNull())
                border = Object.AddComponentOnNewChild<Line>("Border", out _);
            border.SetThickness(ViewSettings.LineWidth * CoordinateConverter.Scale)
                .SetEndCaps(LineEndCap.None)
                .SetColor(GetBorderColor())
                .SetSortingOrder(SortingOrders.PathLine)
                .SetDashType(DashType.Rounded)
                .SetDashSnap(DashSnapping.Off)
                .SetDashSpace(DashSpace.FixedCount);
            (border.Start, border.End, border.Dashed) = GetBorderPointsAndDashed(_Side, false, false);
            border.transform.position = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            border.enabled = false;
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
            var angles = GetCornerAngles(_Right, _Up, _Inner);
            bool isOuterAndNearTrapIncreasing = IsCornerOuterAndNearTurret(_Right, _Up, _Inner);
            var pos = ContainersGetter.GetContainer(ContainerNames.MazeItems).transform.position;
            corner.SetType(DiscType.Arc)
                .SetArcEndCaps(isOuterAndNearTrapIncreasing ? ArcEndCap.Round : ArcEndCap.None)
                .SetRadius(ViewSettings.CornerRadius * CoordinateConverter.Scale)
                .SetThickness(ViewSettings.CornerWidth * CoordinateConverter.Scale)
                .SetAngRadiansStart(Mathf.Deg2Rad * angles.x)
                .SetAngRadiansEnd(Mathf.Deg2Rad * angles.y)
                .SetColor(ColorProvider.GetColor(ColorIds.Main))
                .SetSortingOrder(SortingOrders.PathJoint)
                .transform.SetPosXY(pos)
                .PlusLocalPosXY(GetCornerCenter(_Right, _Up, _Inner));
            corner!.enabled = false;
            const EMazeMoveDirection left = EMazeMoveDirection.Left;
            const EMazeMoveDirection right = EMazeMoveDirection.Right;
            const EMazeMoveDirection down = EMazeMoveDirection.Down;
            const EMazeMoveDirection up = EMazeMoveDirection.Up;
            if (!_Right && !_Up)
            {
                m_BottomLeftCorner = corner;
                BottomLeftCornerInited = true;
                IsBottomLeftCornerInner = _Inner;
                if (!_Inner) return;
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.Start = GetBorderPointsAndDashed(
                        down, true, true).Item1;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.Start = GetBorderPointsAndDashed(left, true, true).Item1;
            }
            else if (_Right && !_Up)
            {
                m_BottomRightCorner = corner;
                BottomRightCornerInited = true;
                IsBottomRightCornerInner = _Inner;
                if (!_Inner) return;
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.End = GetBorderPointsAndDashed(down, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.Start = GetBorderPointsAndDashed(right, true, true).Item1;
            }
            else if (!_Right)
            {
                m_TopLeftCorner = corner;
                TopLeftCornerInited = true;
                IsTopLeftCornerInner = _Inner;
                if (!_Inner) return;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.End = GetBorderPointsAndDashed(left, true, true).Item2;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.Start = GetBorderPointsAndDashed(up, true, true).Item1;
            }
            else
            {
                m_TopRightCorner = corner;
                TopRightCornerInited = true;
                IsTopRightCornerInner = _Inner;
                if (!_Inner) return;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.End = GetBorderPointsAndDashed(up, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.End = GetBorderPointsAndDashed(right, true, true).Item2;
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

        private Tuple<Vector2, Vector2, bool> GetBorderPointsAndDashed(
            EMazeMoveDirection _Side,
            bool _StartLimit, 
            bool _EndLimit)
        {
            Vector2 start, end;
            Vector2 pos = Props.Position;
            float cr = ViewSettings.CornerRadius;
            var left  = Vector2.left;
            var right = Vector2.right;
            var down  = Vector2.down;
            var up    = Vector2.up;
            var zero  = Vector2.zero;
            const float scale = 1f + AdditionalScale;
            switch (_Side)
            {
                case EMazeMoveDirection.Up:
                    start = pos + (up + left * scale) * 0.5f + (_StartLimit ? right * cr : zero);
                    end = pos + (up + right * scale) * 0.5f + (_EndLimit ? left * cr : zero);
                    break;
                case EMazeMoveDirection.Right:
                    start = pos + (right +down * scale) * 0.5f + (_StartLimit ? up * cr : zero);
                    end = pos + (right + up * scale) * 0.5f + (_EndLimit ?down * cr : zero);
                    break;
                case EMazeMoveDirection.Down:
                    start = pos + (down + left * scale) * 0.5f + (_StartLimit ? right * cr : zero);
                    end = pos + (down + right * scale) * 0.5f + (_EndLimit ? left * cr : zero);
                    break;
                case EMazeMoveDirection.Left:
                    start = pos + (left +down * scale) * 0.5f + (_StartLimit ? up * cr : zero);
                    end = pos + (left + up * scale) * 0.5f + (_EndLimit ?down * cr : zero);
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

        private bool IsCornerOuterAndNearTurret(bool _Right, bool _Up, bool _Inner)
        {
            if (_Inner)
                return false;
            bool result = TurretExist(Props.Position + (_Right ? V2Int.Right : V2Int.Left))
                          || TurretExist(Props.Position + (_Up ? V2Int.Up : V2Int.Down));
            return result;
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
            var moneyItemCol = ColorProvider.GetColor(ColorIds.MoneyItem);
            var pathItemCol = ColorProvider.GetColor(GetPathItemColorId());
            if (!Props.Blank && _Appear && !Props.IsStartNode 
                || !_Appear && !Collected)
            {
                if (Props.IsMoneyItem)
                    result.Add(MoneyItem.Renderers, () => moneyItemCol);
                else if (!Collected)
                    result.Add(new [] {m_PathItem}, () => pathItemCol);
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
            var borderCol = GetBorderColor();
            var borders = new List<Component>();
            if (m_BottomBorderInited)
                borders.Add(m_BottomBorder);
            if (m_TopBorderInited)
                borders.Add(m_TopBorder);
            if (m_LeftBorderInited)
                borders.Add(m_LeftBorder);
            if (m_RightBorderInited)
                borders.Add(m_RightBorder);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>> {{borders, () => borderCol}};
            return sets;
        }
        
        private Dictionary<IEnumerable<Component>, Func<Color>> GetCornerAppearSets()
        {
            var setsRaw = new Dictionary<Color, List<Component>>();
            var defCornerCol = ColorProvider.GetColor(ColorIds.Main);
            setsRaw.Add(defCornerCol, new List<Component>());
            if (BottomLeftCornerInited)
                setsRaw[defCornerCol].Add(m_BottomLeftCorner);
            if (BottomRightCornerInited)
                setsRaw[defCornerCol].Add(m_BottomRightCorner);
            if (TopLeftCornerInited)
                setsRaw[defCornerCol].Add(m_TopLeftCorner);
            if (TopRightCornerInited)
                setsRaw[defCornerCol].Add(m_TopRightCorner);
            var sets = setsRaw.
                ToDictionary(
                    _Kvp => (IEnumerable<Component>)_Kvp.Value,
                    _Kvp => new Func<Color>(() => _Kvp.Key));
            return sets;
        }

        protected virtual Color GetBorderColor()
        {
            return ColorProvider.GetColor(ColorIds.Main).SetA(0.5f);
        }

        protected virtual int GetPathItemColorId()
        {
            return ColorIds.PathItem;
        }
        
        protected bool IsAnyBlockWithSamePosition(EMazeItemType _Type)
        {
            return Model.GetAllProceedInfos().Any(_I =>
                _I.Type == _Type && _I.StartPosition == Props.Position);
        }

        #endregion
    }
}
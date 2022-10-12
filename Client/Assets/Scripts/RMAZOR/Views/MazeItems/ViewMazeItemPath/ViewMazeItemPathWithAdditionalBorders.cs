using System;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public class ViewMazeItemPathWithAdditionalBorders : ViewMazeItemPath
    {
        #region constants

        #endregion

        #region nonpublic members

        private bool m_LeftBorder2Inited, m_RightBorder2Inited, m_BottomBorder2Inited, m_TopBorder2Inited;
        private Line m_LeftBorder2,       m_RightBorder2,       m_BottomBorder2,       m_TopBorder2;

        #endregion

        #region inject

        protected ViewMazeItemPathWithAdditionalBorders(
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

        public override Component[] Renderers => base.Renderers
            .Concat(new Component[] {m_LeftBorder2, m_RightBorder2, m_BottomBorder2, m_TopBorder2})
            .ToArray();
        
        public override object Clone() => new ViewMazeItemPathWithAdditionalBorders(
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

        protected override void HighlightBordersAndCorners()
        {
            base.HighlightBordersAndCorners();
            var col = GetHighlightColor();
            if (m_LeftBorder2Inited   && m_LeftBorder2.Color.a > MathUtils.Epsilon)   m_LeftBorder2.Color   = col;
            if (m_RightBorder2Inited  && m_RightBorder2.Color.a > MathUtils.Epsilon)  m_RightBorder2.Color  = col;
            if (m_BottomBorder2Inited && m_BottomBorder2.Color.a > MathUtils.Epsilon) m_BottomBorder2.Color = col;
            if (m_TopBorder2Inited    && m_TopBorder2.Color.a > MathUtils.Epsilon)    m_TopBorder2.Color    = col;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            const EMazeMoveDirection left  = EMazeMoveDirection.Left;
            const EMazeMoveDirection right = EMazeMoveDirection.Right;
            const EMazeMoveDirection down  = EMazeMoveDirection.Down;
            const EMazeMoveDirection up    = EMazeMoveDirection.Up;
            var borderCol = GetBorderColor();
            if (m_LeftBorder2Inited)
            {
                m_LeftBorder2.Color = borderCol.SetA(IsBorderNearTrapReact(left) 
                                                     || IsBorderNearTrapIncreasing(left) ? 0f : 1f);
            }
            if (m_RightBorder2Inited)
            {
                m_RightBorder2.Color = borderCol.SetA(IsBorderNearTrapReact(right) 
                                                      || IsBorderNearTrapIncreasing(right) ? 0f : 1f);
            }
            if (m_BottomBorder2Inited)
            {
                m_BottomBorder2.Color = borderCol.SetA(IsBorderNearTrapReact(down) 
                                                       || IsBorderNearTrapIncreasing(down) ? 0f : 1f);
            }
            if (m_TopBorder2Inited)
            {
                m_TopBorder2.Color = borderCol.SetA(IsBorderNearTrapReact(up) 
                                                    || IsBorderNearTrapIncreasing(up) ? 0f : 1f);
            }
        }

        protected override void DrawBorders()
        {
            base.DrawBorders();
            if (m_LeftBorder2Inited)    m_LeftBorder2.enabled   = false;
            if (m_RightBorder2Inited)   m_RightBorder2.enabled  = false;
            if (m_BottomBorder2Inited)  m_BottomBorder2.enabled = false;
            if (m_TopBorder2Inited)     m_TopBorder2.enabled    = false;
            m_LeftBorder2Inited = m_RightBorder2Inited = m_BottomBorder2Inited = m_TopBorder2Inited = false;
            var sides = Enum
                .GetValues(typeof(EMazeMoveDirection))
                .Cast<EMazeMoveDirection>();
            foreach (var side in sides)
            {
                if (MustInitBorder(side))
                    DrawAdditionalBorder(side);
            }
        }

        protected override void AdjustBorders()
        {
            base.AdjustBorders();
            var pos = Props.Position;
            AdjustAdditionalBorder(pos, V2Int.Left, m_LeftBorder2Inited,
                ref m_LeftBorder2, out bool adjStart, out bool adjEnd);
            if (m_LeftBorder2Inited)
                m_LeftBorder2.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
            AdjustAdditionalBorder(pos, V2Int.Right, m_RightBorder2Inited,
                ref m_RightBorder2, out adjStart, out adjEnd);
            if (m_RightBorder2Inited)
                m_RightBorder2.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
            AdjustAdditionalBorder(pos, V2Int.Down, m_BottomBorder2Inited,
                ref m_BottomBorder2, out adjStart, out adjEnd);
            if (m_BottomBorder2Inited)
                m_BottomBorder2.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
            AdjustAdditionalBorder(pos, V2Int.Up, m_TopBorder2Inited,
                ref m_TopBorder2, out adjStart, out adjEnd);
            if (m_TopBorder2Inited)
                m_TopBorder2.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
        }
        
        private void DrawAdditionalBorder(EMazeMoveDirection _Side)
        {
            Line border = _Side switch
            {
                EMazeMoveDirection.Up    => m_TopBorder2,
                EMazeMoveDirection.Right => m_RightBorder2,
                EMazeMoveDirection.Down  => m_BottomBorder2,
                EMazeMoveDirection.Left  => m_LeftBorder2,
                _                        => throw new SwitchCaseNotImplementedException(_Side)
            };
            if (border.IsNull())
                border = Object.AddComponentOnNewChild<Line>("Additional Border", out _);
            border.SetThickness(ViewSettings.LineThickness * CoordinateConverter.Scale)
                .SetEndCaps(LineEndCap.Round)
                .SetColor(GetBorderColor())
                .SetSortingOrder(SortingOrders.PathLine)
                .SetDashed(true)
                .SetDashType(DashType.Angled)
                .SetDashSnap(DashSnapping.Off)
                .SetDashSpace(DashSpace.FixedCount)
                .SetDashShapeModifier(1f);
            (border.Start, border.End) = GetAdditionalBorderPoints(_Side, false, false);
            border.transform.position = ContainersGetter.GetContainer(
                ContainerNames.MazeItems).transform.position;
            border.enabled = false;
            switch (_Side)
            {
                case EMazeMoveDirection.Up:    m_TopBorder2    = border; m_TopBorder2Inited    = true; break;
                case EMazeMoveDirection.Right: m_RightBorder2  = border; m_RightBorder2Inited  = true; break;
                case EMazeMoveDirection.Down:  m_BottomBorder2 = border; m_BottomBorder2Inited = true; break;
                case EMazeMoveDirection.Left:  m_LeftBorder2   = border; m_LeftBorder2Inited   = true; break;
                default:                       throw new SwitchCaseNotImplementedException(_Side);
            }
        }
        
        protected override void AdjustBordersOnCornerInitialization(
            bool _Right,
            bool _Up,
            bool _Inner)
        {
            base.AdjustBordersOnCornerInitialization(_Right, _Up, _Inner);
            const EMazeMoveDirection left  = EMazeMoveDirection.Left;
            const EMazeMoveDirection right = EMazeMoveDirection.Right;
            const EMazeMoveDirection down  = EMazeMoveDirection.Down;
            const EMazeMoveDirection up    = EMazeMoveDirection.Up;
            if (!_Inner)
                return;
            if (!_Right && !_Up)
            {
                if (m_BottomBorder2.IsNotNull())
                    m_BottomBorder2.Start = GetAdditionalBorderPoints(down, true, true).Item1;
                if (m_LeftBorder2.IsNotNull())
                    m_LeftBorder2.Start = GetAdditionalBorderPoints(left, true, true).Item1;
            }
            else if (_Right && !_Up)
            {
                if (m_BottomBorder2.IsNotNull())
                    m_BottomBorder2.End = GetAdditionalBorderPoints(down, true, true).Item2;
                if (m_RightBorder2.IsNotNull())
                    m_RightBorder2.Start = GetAdditionalBorderPoints(right, true, true).Item1;
            }
            else if (!_Right)
            {
                if (m_LeftBorder2.IsNotNull())
                    m_LeftBorder2.End = GetAdditionalBorderPoints(left, true, true).Item2;
                if (m_TopBorder2.IsNotNull())
                    m_TopBorder2.Start = GetAdditionalBorderPoints(up, true, true).Item1;
            }
            else
            {
                if (m_TopBorder2.IsNotNull())
                    m_TopBorder2.End = GetAdditionalBorderPoints(up, true, true).Item2;
                if (m_RightBorder2.IsNotNull())
                    m_RightBorder2.End = GetAdditionalBorderPoints(right, true, true).Item2;
            }
        }
        
        private void AdjustAdditionalBorder(
            V2Int                  _Position,
            V2Int                  _Direction,
            bool                   _BorderInitialized,
            ref Line               _Border,
            out bool               _AdjustStart,
            out bool               _AdjustEnd)
        {
            var dir1 = _Direction == V2Int.Left || _Direction == V2Int.Right ? V2Int.Down : V2Int.Left;
            var dir2 = _Direction == V2Int.Left || _Direction == V2Int.Right ? V2Int.Up : V2Int.Right;
            _AdjustStart = false;
            _AdjustEnd = false;
            if (PathExist(_Position + _Direction) || !_BorderInitialized) 
                return;
            var dir = RmazorUtils.GetMoveDirection(_Direction, EMazeOrientation.North);
            var (start, end) = GetAdditionalBorderPoints(dir, true, true);
            if (PathExist(_Position + _Direction + dir1)
                || TurretExist(_Position + _Direction + dir1))
            {
                (_AdjustStart, _Border.Start) = (true, start);
            }
            if (PathExist(_Position + _Direction + dir2)
                || TurretExist(_Position + _Direction + dir2))
            {
                (_AdjustEnd, _Border.End) = (true, end);
            }
            if (!PathExist(_Position + dir1))
            {
                (_AdjustStart, _Border.Start) = (true, start);
            }
            _Border.DashSize = 8f * Vector3.Distance(_Border.Start, _Border.End) / CoordinateConverter.Scale;
        }

        private Tuple<Vector2, Vector2> GetAdditionalBorderPoints(
            EMazeMoveDirection _Side,
            bool               _StartLimit,
            bool               _EndLimit)
        {
            Vector2 start, end;
            Vector2 left, right, down, up;
            (left, right, down, up) = (Vector2.left, Vector2.right, Vector2.down, Vector2.up);
            float sb = ViewSettings.LineThickness;
            (start, end) = GetBorderPointsRaw(_Side, _StartLimit, _EndLimit, 1f);
            Vector2 addict = _Side switch
            {
                EMazeMoveDirection.Up    => up * sb,
                EMazeMoveDirection.Right => right * sb,
                EMazeMoveDirection.Down  => down * sb,
                EMazeMoveDirection.Left  => left * sb,
                _                        => default
            };
            start += addict;
            end += addict;
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);
            return new Tuple<Vector2, Vector2>(start, end);
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetBorderAppearSets()
        {
            var col = GetBorderColor();
            var bottomBorder2Col = col.SetA(IsBorderNearTrapReact(EMazeMoveDirection.Down) || IsBorderNearTrapIncreasing(EMazeMoveDirection.Down) ? 0f : 1f);
            var topBorder2Col = col.SetA(IsBorderNearTrapReact(EMazeMoveDirection.Up) || IsBorderNearTrapIncreasing(EMazeMoveDirection.Up) ? 0f : 1f);
            var leftBorder2Col = col.SetA(IsBorderNearTrapReact(EMazeMoveDirection.Left) || IsBorderNearTrapIncreasing(EMazeMoveDirection.Left) ? 0f : 1f);
            var rightBorder2Col = col.SetA(IsBorderNearTrapReact(EMazeMoveDirection.Right) || IsBorderNearTrapIncreasing(EMazeMoveDirection.Right) ? 0f : 1f);
            var sets = base.GetBorderAppearSets();
            if (m_BottomBorder2Inited) sets.Add(new [] {m_BottomBorder2}, () => bottomBorder2Col);
            if (m_TopBorder2Inited)    sets.Add(new [] {m_TopBorder2}, () => topBorder2Col);
            if (m_LeftBorder2Inited)   sets.Add(new [] {m_LeftBorder2}, () => leftBorder2Col);
            if (m_RightBorder2Inited)  sets.Add(new [] {m_RightBorder2}, () => rightBorder2Col);
            return sets;
        }

        protected override void EnableInitializedShapes(bool _Enable)
        {
            base.EnableInitializedShapes(_Enable);
            if (m_LeftBorder2Inited)   m_LeftBorder2.enabled   = _Enable;
            if (m_RightBorder2Inited)  m_RightBorder2.enabled  = _Enable;
            if (m_BottomBorder2Inited) m_BottomBorder2.enabled = _Enable;
            if (m_TopBorder2Inited)    m_TopBorder2.enabled    = _Enable;
        }

        #endregion
    }
}
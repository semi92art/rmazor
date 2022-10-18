using System;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathExtraBorders1 : IViewMazeItemPathExtraBorders { }
    
    public class ViewMazeItemPathExtraBorders1 : 
        ViewMazeItemPathExtraBordersBase,
        IViewMazeItemPathExtraBorders1
    {
        #region nonpublic members
        
        private Line
            m_LeftBorder,   
            m_RightBorder,   
            m_BottomBorder,   
            m_TopBorder;

        #endregion

        #region inject

        private ViewMazeItemPathExtraBorders1(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IColorProvider              _ColorProvider,
            IViewMazeItemsPathInformer  _Informer) 
            : base(
                _ViewSettings, 
                _Model,
                _CoordinateConverter, 
                _ContainersGetter,
                _ColorProvider,
                _Informer) { }
        #endregion

        #region api

        public override bool Activated
        {
            get => base.Activated;
            set
            {
                static void EnableBorder(Behaviour _Border, bool _Enable, bool _Inited)
                {
                    if (_Inited)
                        _Border.enabled = _Enable;
                }
                EnableBorder(m_LeftBorder, value,  LeftExtraBordersInited);
                EnableBorder(m_RightBorder,value,  RightExtraBordersInited);
                EnableBorder(m_BottomBorder,value, BottomExtraBordersInited);
                EnableBorder(m_TopBorder,value,    TopExtraBordersInited);
                base.Activated = value;
            }
        }

        public override Component[] Renderers => new Component[]
        {
            m_LeftBorder,
            m_RightBorder, 
            m_BottomBorder, 
            m_TopBorder
        };
        
        public override object Clone() => new ViewMazeItemPathExtraBorders1(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            ColorProvider,
            Informer.Clone() as IViewMazeItemsPathInformer);

        public override void EnableInitializedShapes(bool _Enable)
        {
            if (LeftExtraBordersInited)   m_LeftBorder  .enabled = _Enable;
            if (RightExtraBordersInited)  m_RightBorder .enabled = _Enable;
            if (BottomExtraBordersInited) m_BottomBorder.enabled = _Enable;
            if (TopExtraBordersInited)    m_TopBorder   .enabled = _Enable;
        }

        public override void HighlightBordersAndCorners()
        {
            var col = Informer.GetHighlightColor();
            if (LeftExtraBordersInited   && m_LeftBorder  .enabled) m_LeftBorder  .Color = col;
            if (RightExtraBordersInited  && m_RightBorder .enabled) m_RightBorder .Color = col;
            if (BottomExtraBordersInited && m_BottomBorder.enabled) m_BottomBorder.Color = col;
            if (TopExtraBordersInited    && m_TopBorder   .enabled) m_TopBorder   .Color = col;
        }

        public override void DrawBorders()
        {
            if (LeftExtraBordersInited)    m_LeftBorder  .enabled = false;
            if (RightExtraBordersInited)   m_RightBorder .enabled = false;
            if (BottomExtraBordersInited)  m_BottomBorder.enabled = false;
            if (TopExtraBordersInited)     m_TopBorder   .enabled = false;
            LeftExtraBordersInited = RightExtraBordersInited = BottomExtraBordersInited = TopExtraBordersInited = false;
            var sides = Enum
                .GetValues(typeof(EMazeMoveDirection))
                .Cast<EMazeMoveDirection>();
            foreach (var side in sides)
            {
                if (Informer.MustInitBorder(side) && Activated)
                    DrawExtraBorder(side);
            }
        }

        public override void AdjustBorders()
        {
            var pos = GetProps().Position;
            AdjustAdditionalBorder(pos, V2Int.Left, LeftExtraBordersInited,
                ref m_LeftBorder, out bool adjStart, out bool adjEnd);
            if (LeftExtraBordersInited)
                m_LeftBorder.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
            AdjustAdditionalBorder(pos, V2Int.Right, RightExtraBordersInited,
                ref m_RightBorder, out adjStart, out adjEnd);
            if (RightExtraBordersInited)
                m_RightBorder.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
            AdjustAdditionalBorder(pos, V2Int.Down, BottomExtraBordersInited,
                ref m_BottomBorder, out adjStart, out adjEnd);
            if (BottomExtraBordersInited)
                m_BottomBorder.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
            AdjustAdditionalBorder(pos, V2Int.Up, TopExtraBordersInited,
                ref m_TopBorder, out adjStart, out adjEnd);
            if (TopExtraBordersInited)
                m_TopBorder.SetDashOffset(adjStart && !adjEnd ? 0.8f : 0f);
        }

        public override void AdjustBordersOnCornerInitialization(bool _Right, bool _Up, bool _Inner)
        {
            const EMazeMoveDirection left  = EMazeMoveDirection.Left;
            const EMazeMoveDirection right = EMazeMoveDirection.Right;
            const EMazeMoveDirection down  = EMazeMoveDirection.Down;
            const EMazeMoveDirection up    = EMazeMoveDirection.Up;
            if (!_Inner)
                return;
            if (!_Right && !_Up)
            {
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.Start = GetAdditionalBorderPoints(down, true, true).Item1;
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.Start = GetAdditionalBorderPoints(left, true, true).Item1;
            }
            else if (_Right && !_Up)
            {
                if (m_BottomBorder.IsNotNull())
                    m_BottomBorder.End = GetAdditionalBorderPoints(down, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.Start = GetAdditionalBorderPoints(right, true, true).Item1;
            }
            else if (!_Right)
            {
                if (m_LeftBorder.IsNotNull())
                    m_LeftBorder.End = GetAdditionalBorderPoints(left, true, true).Item2;
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.Start = GetAdditionalBorderPoints(up, true, true).Item1;
            }
            else
            {
                if (m_TopBorder.IsNotNull())
                    m_TopBorder.End = GetAdditionalBorderPoints(up, true, true).Item2;
                if (m_RightBorder.IsNotNull())
                    m_RightBorder.End = GetAdditionalBorderPoints(right, true, true).Item2;
            }
        }
        
        public override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = GetBorderColor();
            Color GetBorderColorWithAlpha(EMazeMoveDirection _Side)
            {
                float alphaChannel = Informer.IsBorderNearTrapReact(_Side)
                                     || Informer.IsBorderNearTrapIncreasing(_Side)
                    ? 0f : 1f;
                return col.SetA(alphaChannel);
            }
            var colorLeft   = GetBorderColorWithAlpha(EMazeMoveDirection.Left);
            var colorRight  = GetBorderColorWithAlpha(EMazeMoveDirection.Right);
            var colorBottom = GetBorderColorWithAlpha(EMazeMoveDirection.Down);
            var colorTop    = GetBorderColorWithAlpha(EMazeMoveDirection.Up);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>();
            if (LeftExtraBordersInited)   sets.Add(new [] {m_LeftBorder},   () => colorLeft);
            if (RightExtraBordersInited)  sets.Add(new [] {m_RightBorder},  () => colorRight);
            if (BottomExtraBordersInited) sets.Add(new [] {m_BottomBorder}, () => colorBottom);
            if (TopExtraBordersInited)    sets.Add(new [] {m_TopBorder},    () => colorTop);
            return sets;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            var borderCol = GetBorderColor();
            void SetBorderColor(ShapeRenderer _Border, bool _Inited, EMazeMoveDirection _Side)
            {
                if (!_Inited)
                    return;
                float alphaChannel = 
                    Informer.IsBorderNearTrapReact(_Side) 
                    || Informer.IsBorderNearTrapIncreasing(_Side)
                    ? 0f : 1f;
                _Border.Color = borderCol.SetA(alphaChannel);
            }
            SetBorderColor(m_LeftBorder,   LeftExtraBordersInited,   EMazeMoveDirection.Left);
            SetBorderColor(m_RightBorder,  RightExtraBordersInited,  EMazeMoveDirection.Right);
            SetBorderColor(m_BottomBorder, BottomExtraBordersInited, EMazeMoveDirection.Down);
            SetBorderColor(m_TopBorder,    TopExtraBordersInited,    EMazeMoveDirection.Up);
        }

        private void DrawExtraBorder(EMazeMoveDirection _Side)
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
                border = GetParent().AddComponentOnNewChild<Line>("Additional Border", out _);
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
            switch (_Side)
            {
                case EMazeMoveDirection.Left:  m_LeftBorder   = border; LeftExtraBordersInited   = true; break;
                case EMazeMoveDirection.Right: m_RightBorder  = border; RightExtraBordersInited  = true; break;
                case EMazeMoveDirection.Down:  m_BottomBorder = border; BottomExtraBordersInited = true; break;
                case EMazeMoveDirection.Up:    m_TopBorder    = border; TopExtraBordersInited    = true; break;
                default:                       throw new SwitchCaseNotImplementedException(_Side);
            }
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
            (start, end) = Informer.GetBorderPointsRaw(
                _Side,
                _StartLimit, 
                _EndLimit, 
                1f);
            Vector2 addict = _Side switch
            {
                EMazeMoveDirection.Left  => left  * sb,
                EMazeMoveDirection.Right => right * sb,
                EMazeMoveDirection.Down  => down  * sb,
                EMazeMoveDirection.Up    => up    * sb,
                _                        => default
            };
            start += addict;
            end += addict;
            start = CoordinateConverter.ToLocalMazeItemPosition(start);
            end = CoordinateConverter.ToLocalMazeItemPosition(end);
            return new Tuple<Vector2, Vector2>(start, end);
        }
        
        private void AdjustAdditionalBorder(
            V2Int    _Position,
            V2Int    _Direction,
            bool     _BorderInitialized,
            ref Line _Border,
            out bool _AdjustStart,
            out bool _AdjustEnd)
        {
            var dir1 = _Direction == V2Int.Left || _Direction == V2Int.Right ? V2Int.Down : V2Int.Left;
            var dir2 = _Direction == V2Int.Left || _Direction == V2Int.Right ? V2Int.Up : V2Int.Right;
            _AdjustStart = false;
            _AdjustEnd = false;
            if (Informer.PathExist(_Position + _Direction) || !_BorderInitialized) 
                return;
            var dir = RmazorUtils.GetMoveDirection(_Direction, EMazeOrientation.North);
            var (start, end) = GetAdditionalBorderPoints(dir, true, true);
            if (Informer.PathExist(_Position + _Direction + dir1)
                || Informer.TurretExist(_Position + _Direction + dir1))
            {
                (_AdjustStart, _Border.Start) = (true, start);
            }
            if (Informer.PathExist(_Position + _Direction + dir2)
                || Informer.TurretExist(_Position + _Direction + dir2))
            {
                (_AdjustEnd, _Border.End) = (true, end);
            }
            if (!Informer.PathExist(_Position + dir1))
            {
                (_AdjustStart, _Border.Start) = (true, start);
            }
            _Border.DashSize = 8f * Vector3.Distance(_Border.Start, _Border.End) / CoordinateConverter.Scale;
        }

        #endregion
    }
}
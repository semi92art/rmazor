using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathExtraBorders2 : IViewMazeItemPathExtraBorders { }
    
    public class ViewMazeItemPathExtraBorders2 :
        ViewMazeItemPathExtraBordersBase,
        IViewMazeItemPathExtraBorders2
    {
        #region nonpublic members

        private List<Rectangle>
            m_LeftRectangles   = new List<Rectangle>(),
            m_RightRectangles  = new List<Rectangle>(),
            m_BottomRectangles = new List<Rectangle>(),
            m_TopRectangles    = new List<Rectangle>();

        #endregion

        #region inject
        
        private ViewMazeItemPathExtraBorders2(
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
                static void EnableBorder(List<Rectangle> _Borders, bool _Enable, bool _Inited)
                {
                    if (_Inited)
                        _Borders.ForEach(_Border => _Border.enabled = _Enable);
                }
                EnableBorder(m_LeftRectangles, value,   LeftExtraBordersInited);
                EnableBorder(m_RightRectangles, value,  RightExtraBordersInited);
                EnableBorder(m_BottomRectangles, value, BottomExtraBordersInited);
                EnableBorder(m_TopRectangles, value,    TopExtraBordersInited);
                base.Activated = value;
            }
        }
        
        public override Component[] Renderers =>
            m_LeftRectangles
                .Cast<Component>()
                .Concat(m_RightRectangles)
                .Concat(m_BottomRectangles)
                .Concat(m_TopRectangles)
                .ToArray();

        public override object Clone() => new ViewMazeItemPathExtraBorders2(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            ColorProvider,
            Informer.Clone() as IViewMazeItemsPathInformer);

        public override void EnableInitializedShapes(bool _Enable)
        {
            EnableBorderShapes(_Enable, m_LeftRectangles,   LeftExtraBordersInited);
            EnableBorderShapes(_Enable, m_RightRectangles,  RightExtraBordersInited);
            EnableBorderShapes(_Enable, m_BottomRectangles, BottomExtraBordersInited);
            EnableBorderShapes(_Enable, m_TopRectangles,    TopExtraBordersInited);
        }

        public override void HighlightBordersAndCorners()
        {
            var col = Informer.GetHighlightColor();
            void SetRectsColor(bool _Inited, IEnumerable<ShapeRenderer> _Rectangles)
            {
                if (!_Inited) 
                    return;
                foreach (var rect in _Rectangles)
                {
                    if (rect.enabled)
                        rect.Color = col;
                }
            }
            SetRectsColor(LeftExtraBordersInited,   m_LeftRectangles);
            SetRectsColor(RightExtraBordersInited,  m_RightRectangles);
            SetRectsColor(BottomExtraBordersInited, m_BottomRectangles);
            SetRectsColor(TopExtraBordersInited,    m_TopRectangles);
        }

        public override void DrawBorders()
        {
            static void DisableRects(bool _Inited, IEnumerable<ShapeRenderer> _Rectangles)
            {
                if (!_Inited) 
                    return;
                foreach (var rect in _Rectangles)
                {
                    rect.enabled = false;
                }
            }
            DisableRects(LeftExtraBordersInited,   m_LeftRectangles);
            DisableRects(RightExtraBordersInited,  m_RightRectangles);
            DisableRects(BottomExtraBordersInited, m_BottomRectangles);
            DisableRects(TopExtraBordersInited,    m_TopRectangles);
            LeftExtraBordersInited = RightExtraBordersInited = BottomExtraBordersInited = TopExtraBordersInited = false;
            var sides = Enum
                .GetValues(typeof(EDirection))
                .Cast<EDirection>();
            foreach (var side in sides)
            {
                if (Informer.MustInitBorder(side) && Activated)
                    DrawExtraBorder(side);
            }
        }
        
        public override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = GetBorderColor();
            Color GetBorderColorWithAlpha(EDirection _Side)
            {
                float alphaChannel = Informer.IsBorderNearTrapReact(_Side)
                                     || Informer.IsBorderNearTrapIncreasing(_Side)
                    ? 0f : 1f;
                return col.SetA(alphaChannel);
            }
            var colorLeft   = GetBorderColorWithAlpha(EDirection.Left);
            var colorRight  = GetBorderColorWithAlpha(EDirection.Right);
            var colorBottom = GetBorderColorWithAlpha(EDirection.Down);
            var colorTop    = GetBorderColorWithAlpha(EDirection.Up);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>();
            if (LeftExtraBordersInited)   sets.Add(m_LeftRectangles,   () => colorLeft);
            if (RightExtraBordersInited)  sets.Add(m_RightRectangles,  () => colorRight);
            if (BottomExtraBordersInited) sets.Add(m_BottomRectangles, () => colorBottom);
            if (TopExtraBordersInited)    sets.Add(m_TopRectangles,    () => colorTop);
            return sets;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            var borderCol = GetBorderColor();
            void SetRectsColor(
                bool                       _Inited,
                EDirection         _Direction,
                IEnumerable<ShapeRenderer> _Rectangles)
            {
                if (!_Inited) 
                    return;
                foreach (var rect in _Rectangles)
                {
                    float alphaChannel = Informer.IsBorderNearTrapReact(_Direction)
                                         || Informer.IsBorderNearTrapIncreasing(_Direction)
                        ? 0f : 1f;
                    rect.Color = borderCol.SetA(alphaChannel);
                }
            }
            SetRectsColor(LeftExtraBordersInited,   EDirection.Left,  m_LeftRectangles);
            SetRectsColor(RightExtraBordersInited,  EDirection.Right, m_RightRectangles);
            SetRectsColor(BottomExtraBordersInited, EDirection.Down,  m_BottomRectangles);
            SetRectsColor(TopExtraBordersInited,    EDirection.Up,    m_TopRectangles);
        }

        private void DrawExtraBorder(EDirection _Side)
        {
            List<Rectangle> rects = _Side switch
            {
                EDirection.Left  => m_LeftRectangles,
                EDirection.Right => m_RightRectangles,
                EDirection.Down  => m_BottomRectangles,
                EDirection.Up    => m_TopRectangles,
                _                        => throw new SwitchCaseNotImplementedException(_Side)
            };
            if (!rects.Any())
            {
                var rect1 = GetParent().AddComponentOnNewChild<Rectangle>($"{_Side} Extra Rect 1", out _);
                var rect2 = GetParent().AddComponentOnNewChild<Rectangle>($"{_Side} Extra Rect 2", out _);
                var rect3 = GetParent().AddComponentOnNewChild<Rectangle>($"{_Side} Extra Rect 3", out _);
                rects.AddRange(new [] { rect1, rect2, rect3 });
            }
            float scale = CoordinateConverter.Scale;
            foreach (var rect in rects)
            {
                rect.SetType(Rectangle.RectangleType.RoundedSolid)
                    .SetColor(GetBorderColor())
                    .SetSortingOrder(SortingOrders.PathLine)
                    .SetSize(scale * 0.2f)
                    .SetCornerRadius(scale * 0.25f * ViewSettings.CornerRadius);
            }
            var positions = GetRectsPositions(_Side);
            rects[0].SetSize(scale * 0.3f);
            rects[0].transform.SetPosXY(positions[0]);
            rects[1].transform.SetPosXY(positions[1]);
            rects[2].transform.SetPosXY(positions[2]);
            switch (_Side)
            {
                case EDirection.Left:  m_LeftRectangles   = rects; LeftExtraBordersInited   = true; break;
                case EDirection.Right: m_RightRectangles  = rects; RightExtraBordersInited  = true; break;
                case EDirection.Down:  m_BottomRectangles = rects; BottomExtraBordersInited = true; break;
                case EDirection.Up:    m_TopRectangles    = rects; TopExtraBordersInited    = true; break;
                default:                       throw new SwitchCaseNotImplementedException(_Side);
            }
        }
        
        private List<Vector2> GetRectsPositions(EDirection _Side)
        {
            Vector2 pos = GetProps().Position;
            Vector2 left, right, down, up;
            (left, right, down, up) = (Vector2.left, Vector2.right, Vector2.down, Vector2.up);
            Vector2 pos1, pos2, pos3;
            const float c1 = 0.5f;
            const float c2 = 0.33f;
            const float indent = 0.15f;
            switch (_Side)
            {
                case EDirection.Up:
                    pos1 = pos + (up * c1)    + (up    * indent);
                    pos2 = pos + (up * c1     + left   * c2) + (up    * indent);
                    pos3 = pos + (up * c1     + right  * c2) + (up    * indent);
                    break;
                case EDirection.Right:
                    pos1 = pos + (right * c1) + (right * indent);
                    pos2 = pos + (right * c1  + down   * c2) + (right * indent);
                    pos3 = pos + (right * c1  + up     * c2) + (right * indent);
                    break;
                case EDirection.Down:
                    pos1 = pos + (down * c1) + (down  * indent);
                    pos2 = pos + (down * c1  + left   * c2)  + (down  * indent);
                    pos3 = pos + (down * c1  + right  * c2)  + (down  * indent);
                    break;
                case EDirection.Left:
                    pos1 = pos + (left * c1) + (left  * indent);
                    pos2 = pos + (left * c1  + down   * c2)  + (left  * indent);
                    pos3 = pos + (left * c1  + up     * c2)  + (left  * indent);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Side);
            }
            pos1 = CoordinateConverter.ToGlobalMazeItemPosition(pos1);
            pos2 = CoordinateConverter.ToGlobalMazeItemPosition(pos2);
            pos3 = CoordinateConverter.ToGlobalMazeItemPosition(pos3);
            return new List<Vector2> {pos1, pos2, pos3};
        }
        
        #endregion
    }
}
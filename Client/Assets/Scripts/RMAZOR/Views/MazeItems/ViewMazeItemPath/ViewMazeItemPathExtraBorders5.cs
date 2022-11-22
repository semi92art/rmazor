using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public interface IViewMazeItemPathExtraBorders5 : IViewMazeItemPathExtraBorders { }
    
    public class ViewMazeItemPathExtraBorders5 :
        ViewMazeItemPathExtraBordersBase,
        IViewMazeItemPathExtraBorders5
    {
        #region nonpublic members

        private List<RegularPolygon>
            m_LeftExtraBorders   = new List<RegularPolygon>(),
            m_RightExtraBorders  = new List<RegularPolygon>(),
            m_BottomExtraBorders = new List<RegularPolygon>(),
            m_TopExtraBorders    = new List<RegularPolygon>();

        #endregion

        #region inject
        
        private ViewMazeItemPathExtraBorders5(
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
                static void EnableBorder(List<RegularPolygon> _Borders, bool _Enable, bool _Inited)
                {
                    if (_Inited)
                        _Borders.ForEach(_Border => _Border.enabled = _Enable);
                }
                EnableBorder(m_LeftExtraBorders, value,   LeftExtraBordersInited);
                EnableBorder(m_RightExtraBorders, value,  RightExtraBordersInited);
                EnableBorder(m_BottomExtraBorders, value, BottomExtraBordersInited);
                EnableBorder(m_TopExtraBorders, value,    TopExtraBordersInited);
                base.Activated = value;
            }
        }
        
        public override Component[] Renderers => 
            m_LeftExtraBorders
                .Cast<Component>()
                .Concat(m_RightExtraBorders)
                .Concat(m_BottomExtraBorders)
                .Concat(m_TopExtraBorders)
                .ToArray();

        public override object Clone() => new ViewMazeItemPathExtraBorders5(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            ColorProvider,
            Informer.Clone() as IViewMazeItemsPathInformer);

        public override void EnableInitializedShapes(bool _Enable)
        {
            EnableBorderShapes(_Enable, m_LeftExtraBorders,   LeftExtraBordersInited);
            EnableBorderShapes(_Enable, m_RightExtraBorders,  RightExtraBordersInited);
            EnableBorderShapes(_Enable, m_BottomExtraBorders, BottomExtraBordersInited);
            EnableBorderShapes(_Enable, m_TopExtraBorders,    TopExtraBordersInited);
        }

        public override void HighlightBordersAndCorners()
        {
            var col = Informer.GetHighlightColor();
            void SetPolygonColor(bool _Inited, IEnumerable<ShapeRenderer> _Rectangles)
            {
                if (!_Inited) 
                    return;
                foreach (var rect in _Rectangles)
                {
                    if (rect.enabled)
                        rect.Color = col;
                }
            }
            SetPolygonColor(LeftExtraBordersInited,   m_LeftExtraBorders);
            SetPolygonColor(RightExtraBordersInited,  m_RightExtraBorders);
            SetPolygonColor(BottomExtraBordersInited, m_BottomExtraBorders);
            SetPolygonColor(TopExtraBordersInited,    m_TopExtraBorders);
        }

        public override void DrawBorders()
        {
            static void DisablePolygons(bool _Inited, IEnumerable<ShapeRenderer> _Rectangles)
            {
                if (!_Inited) 
                    return;
                foreach (var rect in _Rectangles)
                {
                    rect.enabled = false;
                }
            }
            DisablePolygons(LeftExtraBordersInited,   m_LeftExtraBorders);
            DisablePolygons(RightExtraBordersInited,  m_RightExtraBorders);
            DisablePolygons(BottomExtraBordersInited, m_BottomExtraBorders);
            DisablePolygons(TopExtraBordersInited,    m_TopExtraBorders);
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
            if (LeftExtraBordersInited)   sets.Add(m_LeftExtraBorders,   () => colorLeft);
            if (RightExtraBordersInited)  sets.Add(m_RightExtraBorders,  () => colorRight);
            if (BottomExtraBordersInited) sets.Add(m_BottomExtraBorders, () => colorBottom);
            if (TopExtraBordersInited)    sets.Add(m_TopExtraBorders,    () => colorTop);
            return sets;
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            var borderCol = GetBorderColor();
            void SetPolygonsColor(
                bool                       _Inited,
                EDirection         _Direction,
                IEnumerable<ShapeRenderer> _Rectangles)
            {
                if (!_Inited) 
                    return;
                foreach (var rect in _Rectangles)
                {
                    rect.Color = borderCol.SetA(Informer.IsBorderNearTrapReact(_Direction) 
                                                || Informer.IsBorderNearTrapIncreasing(_Direction) ? 0f : 1f);
                }
            }
            SetPolygonsColor(LeftExtraBordersInited,   EDirection.Left,  m_LeftExtraBorders);
            SetPolygonsColor(RightExtraBordersInited,  EDirection.Right, m_RightExtraBorders);
            SetPolygonsColor(BottomExtraBordersInited, EDirection.Down,  m_BottomExtraBorders);
            SetPolygonsColor(TopExtraBordersInited,    EDirection.Up,    m_TopExtraBorders);
        }

        private void DrawExtraBorder(EDirection _Side)
        {
            List<RegularPolygon> polygons = _Side switch
            {
                EDirection.Left  => m_LeftExtraBorders,
                EDirection.Right => m_RightExtraBorders,
                EDirection.Down  => m_BottomExtraBorders,
                EDirection.Up    => m_TopExtraBorders,
                _                => throw new SwitchCaseNotImplementedException(_Side)
            };
            if (!polygons.Any())
            {
                var polygon1 = GetParent().AddComponentOnNewChild<RegularPolygon>($"{_Side} Extra Rect 1", out _);
                var polygon2 = GetParent().AddComponentOnNewChild<RegularPolygon>($"{_Side} Extra Rect 2", out _);
                var polygon3 = GetParent().AddComponentOnNewChild<RegularPolygon>($"{_Side} Extra Rect 3", out _);
                polygons.AddRange(new [] { polygon1, polygon2, polygon3 });
            }
            float scale = CoordinateConverter.Scale;
            float polygonAngle = _Side switch
            {
                EDirection.Right => Mathf.Deg2Rad * (0),
                EDirection.Up    => Mathf.Deg2Rad * (90f),
                EDirection.Left  => Mathf.Deg2Rad * (180f),
                EDirection.Down  => Mathf.Deg2Rad * (270f),
                _                => throw new SwitchExpressionException(_Side)
            };
            foreach (var polygon in polygons)
            {
                polygon
                    .SetSides(5)
                    .SetColor(GetBorderColor())
                    .SetSortingOrder(SortingOrders.PathLine)
                    .SetRadius(scale * 0.17f)
                    .SetRoundness(scale * 0.25f * ViewSettings.LineThickness)
                    .SetAngle(polygonAngle);
            }
            var positions = GetPolygonPositions(_Side);
            polygons[0].SetRadius(scale * 0.25f);
            polygons[0].transform.SetPosXY(positions[0]);
            polygons[1].transform.SetPosXY(positions[1]);
            polygons[2].transform.SetPosXY(positions[2]);
            switch (_Side)
            {
                case EDirection.Left:  m_LeftExtraBorders   = polygons; LeftExtraBordersInited   = true; break;
                case EDirection.Right: m_RightExtraBorders  = polygons; RightExtraBordersInited  = true; break;
                case EDirection.Down:  m_BottomExtraBorders = polygons; BottomExtraBordersInited = true; break;
                case EDirection.Up:    m_TopExtraBorders    = polygons; TopExtraBordersInited    = true; break;
                default: throw new SwitchCaseNotImplementedException(_Side);
            }
        }

        private List<Vector2> GetPolygonPositions(EDirection _Side)
        {
            Vector2 pos = GetProps().Position;
            Vector2 left, right, down, up;
            (left, right, down, up) = (Vector2.left, Vector2.right, Vector2.down, Vector2.up);
            Vector2 pos1, pos2, pos3;
            const float c1 = 0.65f;
            const float c2 = 0.33f;
            const float indent = 0.1f;
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
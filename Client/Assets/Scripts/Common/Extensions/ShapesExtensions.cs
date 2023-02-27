using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Common.Extensions
{
    public static class ShapesExtensions
    {
        #region shape renderer

        public static T SetBlendMode<T>(this T _Shape, ShapesBlendMode _BlendMode) where T : ShapeRenderer
        {
            _Shape.BlendMode = _BlendMode;
            return _Shape;
        }
        
        public static T SetRenderQueue<T>(this T _Shape, int _RenderQueue) where T : ShapeRenderer
        {
            _Shape.RenderQueue = _RenderQueue;
            return _Shape;
        }
        
        public static T SetZTest<T>(this T _Shape, CompareFunction _ZTest) where T : ShapeRenderer
        {
            _Shape.ZTest = _ZTest;
            return _Shape;
        }
        
        public static T SetStencilComp<T>(this T _Shape, CompareFunction _StencilComp) where T : ShapeRenderer
        {
            _Shape.StencilComp = _StencilComp;
            return _Shape;
        }
        
        public static T SetStencilOpPass<T>(this T _Shape, StencilOp _StencilOp) where T : ShapeRenderer
        {
            _Shape.StencilOpPass = _StencilOp;
            return _Shape;
        }
        
        public static T SetStencilRefId<T>(this T _Shape, byte _StencilRefId) where T : ShapeRenderer
        {
            _Shape.StencilRefID = _StencilRefId;
            return _Shape;
        }
        
        public static T SetColor<T>(this T _Shape, Color _Color) where T : ShapeRenderer
        {
            _Shape.Color = _Color;
            return _Shape;
        }

        public static T SetSortingOrder<T>(this T _Shape, int _SortingOrder) where T : ShapeRenderer
        {
            _Shape.SortingOrder = _SortingOrder;
            return _Shape;
        }
        
        public static T SetDashed<T>(this T _Shape, bool _Dashed) where T : ShapeRenderer, IDashable
        {
            _Shape.Dashed = _Dashed;
            return _Shape;
        }

        public static T SetDashType<T>(this T _Shape, DashType _DashType) where T : ShapeRenderer, IDashable
        {
            _Shape.DashType = _DashType;
            return _Shape;
        }

        public static T SetDashSize<T>(this T _Shape, float _DashSize) where T : ShapeRenderer, IDashable
        {
            _Shape.DashSize = _DashSize;
            return _Shape;
        }

        public static T SetDashSpacing<T>(this T _Shape, float _DashSpacing) where T : ShapeRenderer, IDashable
        {
            _Shape.DashSpacing = _DashSpacing;
            return _Shape;
        }
        
        public static T SetDashSpace<T>(this T _Shape, DashSpace _DashSpace) where T : ShapeRenderer, IDashable
        {
            _Shape.DashSpace = _DashSpace;
            return _Shape;
        }
        
        public static T SetDashSnap<T>(this T _Shape, DashSnapping _Snap) where T : ShapeRenderer, IDashable
        {
            _Shape.DashSnap = _Snap;
            return _Shape;
        }

        public static T SetMatchDashSpacingToDashSize<T>(this T _Shape, bool _Match) where T : ShapeRenderer, IDashable
        {
            _Shape.MatchDashSpacingToSize = _Match;
            return _Shape;
        }
        
        public static T SetDashOffset<T>(this T _Shape, float _Offset) where T : ShapeRenderer, IDashable
        {
            _Shape.DashOffset = _Offset;
            return _Shape;
        }
        
        public static T SetDashShapeModifier<T>(this T _Shape, float _ShapeModifier) where T : ShapeRenderer, IDashable
        {
            _Shape.DashShapeModifier = _ShapeModifier;
            return _Shape;
        }

        #endregion

        #region disc

        public static Disc SetType(this Disc _Disc, DiscType _DiscType)
        {
            _Disc.Type = _DiscType;
            return _Disc;
        }

        public static Disc SetColorMode(this Disc _Disc, Disc.DiscColorMode _ColorMode)
        {
            _Disc.ColorMode = _ColorMode;
            return _Disc;
        }

        public static Disc SetColorInner(this Disc _Disc, Color _Color)
        {
            _Disc.ColorInner = _Color;
            return _Disc;
        }

        public static Disc SetColorOuter(this Disc _Disc, Color _Color)
        {
            _Disc.ColorOuter = _Color;
            return _Disc;
        }

        public static Disc SetArcEndCaps(this Disc _Disc, ArcEndCap _ArcEndCap)
        {
            _Disc.ArcEndCaps = _ArcEndCap;
            return _Disc;
        }

        public static Disc SetThickness(this Disc _Disc, float _Thickness)
        {
            _Disc.Thickness = _Thickness;
            return _Disc;
        }

        public static Disc SetRadius(this Disc _Disc, float _Radius)
        {
            _Disc.Radius = _Radius;
            return _Disc;
        }

        public static Disc SetAngRadiansStart(this Disc _Disc, float _Angle)
        {
            _Disc.AngRadiansStart = _Angle;
            return _Disc;
        }

        public static Disc SetAngRadiansEnd(this Disc _Disc, float _Angle)
        {
            _Disc.AngRadiansEnd = _Angle;
            return _Disc;
        }

        #endregion

        #region rectangle

        public static Rectangle SetType(this Rectangle _Rectangle, Rectangle.RectangleType _RectangleType)
        {
            _Rectangle.Type = _RectangleType;
            return _Rectangle;
        }

        public static Rectangle SetWidth(this Rectangle _Rectangle, float _Width)
        {
            _Rectangle.Width = _Width;
            return _Rectangle;
        }

        public static Rectangle SetHeight(this Rectangle _Rectangle, float _Height)
        {
            _Rectangle.Height = _Height;
            return _Rectangle;
        }
        
        public static Rectangle SetSize(this Rectangle _Rectangle, float _Size)
        {
            return _Rectangle.SetWidth(_Size).SetHeight(_Size);
        }

        public static Rectangle SetThickness(this Rectangle _Rectangle, float _Thickness)
        {
            _Rectangle.Thickness = _Thickness;
            return _Rectangle;
        }
        
        public static Rectangle SetCornerRadius(this Rectangle _Rectangle, float _CornerRadius)
        {
            _Rectangle.CornerRadius = _CornerRadius;
            return _Rectangle;
        }
        
        public static Rectangle SetCornerRadii(this Rectangle _Rectangle, Vector4 _CornerRadii)
        {
            _Rectangle.CornerRadii = _CornerRadii;
            return _Rectangle;
        }

        public static Rectangle SetCornerRadiusMode(this Rectangle _R, Rectangle.RectangleCornerRadiusMode _Mode)
        {
            _R.CornerRadiusMode = _Mode;
            return _R;
        }

        #endregion

        #region line

        public static Line SetThickness(this Line _Line, float _Thickness)
        {
            _Line.Thickness = _Thickness;
            return _Line;
        }

        public static Line SetStart(this Line _Line, Vector3 _Start)
        {
            _Line.Start = _Start;
            return _Line;
        }

        public static Line SetEnd(this Line _Line, Vector3 _End)
        {
            _Line.End = _End;
            return _Line;
        }

        public static Line SetStartEnd(this Line _Line, Vector3 _Start, Vector3 _End)
        {
            return _Line.SetStart(_Start).SetEnd(_End);
        }

        public static Line SetEndCaps(this Line _Line, LineEndCap _EndCaps)
        {
            _Line.EndCaps = _EndCaps;
            return _Line;
        }

        #endregion

        #region triangle
        
        public static Triangle SetRoundness(this Triangle _Triangle, float _Roundness)
        {
            _Triangle.Roundness = _Roundness;
            return _Triangle;
        }
        
        public static Triangle SetBorder(this Triangle _Triangle, bool _Border)
        {
            _Triangle.Border = _Border;
            return _Triangle;
        }
        
        public static Triangle SetThickness(this Triangle _Triangle, float _Thickness)
        {
            _Triangle.Thickness = _Thickness;
            return _Triangle;
        }

        public static Triangle SetColor(this Triangle _Triangle, Color _Color)
        {
            _Triangle.Color = _Color;
            return _Triangle;
        }
        
        #endregion
        
        #region regular polygon

        public static RegularPolygon SetSides(this RegularPolygon _RegularPolygon, int _Sides)
        {
            _RegularPolygon.Sides = _Sides;
            return _RegularPolygon;
        }
        
        public static RegularPolygon SetRadius(this RegularPolygon _RegularPolygon, float _Radius)
        {
            _RegularPolygon.Radius = _Radius;
            return _RegularPolygon;
        }
        
        public static RegularPolygon SetRoundness(this RegularPolygon _RegularPolygon, float _Roundness)
        {
            _RegularPolygon.Roundness = _Roundness;
            return _RegularPolygon;
        }

        public static RegularPolygon SetAngle(this RegularPolygon _RegularPolygon, float _Angle)
        {
            _RegularPolygon.Angle = _Angle;
            return _RegularPolygon;
        }

        public static RegularPolygon SetColor(this RegularPolygon _RegularPolygon, Color _Color)
        {
            _RegularPolygon.Color = _Color;
            return _RegularPolygon;
        }

        #endregion
    }
}
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Common.Extensions
{
    public static class ShapesExtensions
    {
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

        public static Disc SetDiscType(this Disc _Disc, DiscType _DiscType)
        {
            _Disc.Type = _DiscType;
            return _Disc;
        }

        public static Disc SetArcEndCaps(this Disc _Disc, ArcEndCap _ArcEndCap)
        {
            _Disc.ArcEndCaps = _ArcEndCap;
            return _Disc;
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
    }
}
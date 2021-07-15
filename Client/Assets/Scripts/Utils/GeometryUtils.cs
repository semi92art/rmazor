using System;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public static class GeometryUtils
    {
        #region api
        
        public static bool Intersect(Rect _R0, Rect _R1)
        {
            return _R0.xMin < _R1.xMax &&
                   _R0.xMax > _R1.xMin &&
                   _R0.yMin < _R1.yMax &&
                   _R0.yMax > _R1.yMin;
        }
    
        public static bool LineSegmentsIntersect(Vector2 _P1, Vector2 _P2, Vector2 _P3, Vector2 _P4, ref Vector2 _Result)
        {
            var d = (_P2.x - _P1.x) * (_P4.y - _P3.y) - (_P2.y - _P1.y) * (_P4.x - _P3.x);

            if (Mathf.Approximately(d, 0.0f))
                return false;

            var u = ((_P3.x - _P1.x) * (_P4.y - _P3.y) - (_P3.y - _P1.y) * (_P4.x - _P3.x)) / d;
            var v = ((_P3.x - _P1.x) * (_P2.y - _P1.y) - (_P3.y - _P1.y) * (_P2.x - _P1.x)) / d;

            if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
                return false;

            _Result.x = _P1.x + u * (_P2.x - _P1.x);
            _Result.y = _P1.y + u * (_P2.y - _P1.y);

            return true;
        }
        
        public static bool IsPointInPolygon(Vector2[] _PolyPoints, Vector2 _P)
        {
            var inside = false;
            for (int i = 0; i < _PolyPoints.Length; i++)
            {
                var p1 = _PolyPoints[i];
                var p2 = _PolyPoints[i == 0 ? _PolyPoints.Length - 1 : i - 1];
			
                if ((p1.y <= _P.y && _P.y < p2.y || p2.y <= _P.y && _P.y < p1.y) &&
                    _P.x < (p2.x - p1.x) * (_P.y - p1.y) / (p2.y - p1.y) + p1.x)
                    inside = !inside;
            }

            return inside;
        }
    
        public static bool Intersect(Vector2 _Position, Vector2[] _Vertices)
        {
            float x 		= _Position.x;
            float y 		= _Position.y;
            bool contains 	= false;
            int vertexCount = _Vertices.Length;
            for (int i = 0, j = vertexCount - 1; i < vertexCount; i++)
            {
                Vector2 a = _Vertices[i];
                Vector2 b = _Vertices[j];
                if ((a.y < y && b.y >= y || b.y < y && a.y >= y) && (a.x <= x || b.x <= x))
                {
                    if (a.x + (y - a.y) / (b.y - a.y) * (b.x - a.x) < x)
                        contains = !contains;
                }
                j = i;
            }
            return contains;
        }

        public static bool CirclesIntersect(Vector2 _X0Y0, float _R0, Vector2 _X1Y1, float _R1)
        {
            float a = MathUtils.Pow2(_X0Y0.x - _X1Y1.x) + MathUtils.Pow2(_X0Y0.y - _X1Y1.y);
            return a > MathUtils.Pow2(_R0 - _R1) && a < MathUtils.Pow2(_R0 + _R1);
        }

        public static bool CircleIsInsideOfOtherCircle(Vector2 _X0Y0, float _R0, Vector2 _X1Y1, float _R1)
        {
            float dist = MathUtils.Pow2(_X0Y0.x - _X1Y1.x) + MathUtils.Pow2(_X0Y0.y - _X1Y1.y);
            return dist < MathUtils.Pow2(_R0 - _R1);
        }

        public static Vector2 Middle(params Vector2[] _Points)
        {
            if (_Points == null || !_Points.Any())
                return default;
            Vector2 sum = _Points.Aggregate<Vector2, Vector2>(default, (_Current, _P) => _Current + _P);
            return sum / _Points.Length;
        }

        public static float ZAngle(Vector2 _From, Vector2 _To)
        {
            float angle = Vector2.Angle(Vector2.right, _To - _From);
            angle *= _To.y > _From.y ? 1 : -1;
            return angle;
        }
        
        public static float ZAngle(Vector2 _Vector)
        {
            float angle = Vector2.Angle(Vector2.right, _Vector);
            angle *= _Vector.y > 0 ? 1 : -1;
            return angle;
        }
        
        #endregion
    }
}
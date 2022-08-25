using System;
using System.Linq;
using Common.Extensions;
using UnityEngine;
using Random = System.Random;

namespace Common.Utils
{
    public static class MathUtils
    {
        public const           float         Epsilon   = 1e-5f;
        public static readonly Random RandomGen = new Random();
        
        public static int Lerp(int _A, int _B, float _T)
        {
            return (int)((1 - _T) * _A + _T * _B);
        }
        
        public static long Lerp(long _A, long _B, float _T)
        {
            return (long)((1 - _T) * _A + _T * _B);
        }

        public static float Pow2(float _Value)
        {
            return Mathf.Pow(_Value, 2);
        }

        public static int Max(params int[] _Values)
        {
            return _Values.Max();
        }

        public static long Clamp(long _X, long _Min, long _Max)
        {
            return Math.Min(Math.Max(_X, _Min), _Max);
        }
        
        public static int Clamp(int _X, int _Min, int _Max)
        {
            return Math.Min(Math.Max(_X, _Min), _Max);
        }
        
        public static float Clamp(float _X, float _Min, float _Max)
        {
            return Math.Min(Math.Max(_X, _Min), _Max);
        }

        public static double Fraction(double _Value)
        {
            return _Value - Math.Truncate(_Value);
        }
        
        public static float Fraction(float _Value)
        {
            return Convert.ToSingle(Fraction(Convert.ToDouble(_Value)));
        }

        public static int ClampInverse(int _Val, int _Min, int _Max)
        {
            return _Val > _Max ? _Min : _Val < _Min ? _Max : _Val;
        }

        public static long ClampInverse(long _Val, long _Min, long _Max)
        {
            return _Val > _Max ? _Min : _Val < _Min ? _Max : _Val;
        }
        
        public static float ClampInverse(float _Val, float _Min, float _Max)
        {
            return _Val > _Max ? _Min : _Val < _Min ? _Max : _Val;
        }
        
        public static bool IsInRange(long _Val, long _Min, long _Max)
        {
            return _Val >= _Min && _Val <= _Max;
        }
    
        public static bool IsInRange(int _Val, int _Min, int _Max)
        {
            return _Val >= _Min && _Val <= _Max;
        }
        
        public static bool IsInRange(float _Val, float _Min, float _Max)
        {
            return _Val >= _Min && _Val <= _Max;
        }
        
        public static bool IsInRange(double _Val, double _Min, double _Max)
        {
            return _Val >= _Min && _Val <= _Max;
        }
        
        /// <summary>
        /// Generates random float value in range of 0.0 and 1.0
        /// </summary>
        /// <param name="_Random">Random generator</param>
        /// <returns>Random value in range of 0.0 and 1.0</returns>
        public static float NextFloat(this Random _Random)
        {
            return (float) _Random.NextDouble();
        }
        
        /// <summary>
        /// Generates random float value in range of -1.0 and 1.0
        /// </summary>
        /// <param name="_Random">Random generator</param>
        /// <returns>Random value in range of -1.0 and 1.0</returns>
        public static float NextFloatAlt(this Random _Random)
        {
            return 2f * ((float) _Random.NextDouble() - 0.5f);
        }
        
        //https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        public static float MinDistanceBetweenPointAndSegment(Vector2 _SegStart, Vector2 _SegEnd, Vector2 _Point)
        {
            float l2 = DistanceSquared(_SegStart, _SegEnd);
            if (Mathf.Abs(l2) < Epsilon)
                return Vector2.Distance(_Point, _SegStart);
            float t = Mathf.Max(
                0, Mathf.Min(1, 
                    Vector2.Dot(_Point - _SegStart, _SegEnd - _SegStart) / l2));
            var projection = _SegStart + t * (_SegEnd - _SegStart);
            return Vector2.Distance(_Point, projection);
        }

        /// <summary>
        /// Test whether two line segments intersect. If so, calculate the intersection point.
        /// <see cref="http://stackoverflow.com/a/14143738/292237"/>
        /// </summary>
        /// <param name="_P1">Vector to the start point of p.</param>
        /// <param name="_P2">Vector to the end point of p.</param>
        /// <param name="_Q1">Vector to the start point of q.</param>
        /// <param name="_Q2">Vector to the end point of q.</param>
        /// <param name="_ConsiderCollinearOverlapAsIntersect">Do we consider overlapping lines as intersecting?
        /// </param>
        /// <returns>True if an intersection point was found.</returns>
        public static Vector2? LineSegementsIntersect(
            Vector2 _P1, 
            Vector2 _P2, 
            Vector2 _Q1,
            Vector2 _Q2, 
            bool _ConsiderCollinearOverlapAsIntersect = false)
        {
            var r = _P2 - _P1;
            var s = _Q2 - _Q1;
            var rxs = r.Cross(s);
            var qpxr = (_Q1 - _P1).Cross(r);
            // If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
            if (rxs.IsZero() && qpxr.IsZero())
            {
                // 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
                // then the two lines are overlapping,
                if (_ConsiderCollinearOverlapAsIntersect)
                {
                    var pr1 = Product(_Q1 - _P1, r);
                    var pr2 = Product(r, r);
                    var pr3 = Product(_P1 - _Q1, s);
                    var pr4 = Product(s, s);
                    if (0 <= pr1 && pr1 <= pr2 || 0 <= pr3 && pr3 <= pr4)
                        return null;
                }
                // 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return null;
            }
            // 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
            if (rxs.IsZero() && !qpxr.IsZero())
                return null;
            // t = (q - p) x s / (r x s)
            var t = (_Q1 - _P1).Cross(s)/rxs;
            // u = (q - p) x r / (r x s)
            var u = (_Q1 - _P1).Cross(r)/rxs;
            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point p + t r = q + u s.
            if (!rxs.IsZero() && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                return _P1 + t*r;
            }
            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return null;
        }
        
        private static float Product(Vector2 _V1, Vector2 _V2)
        {
            return _V1.x * _V2.x + _V1.y * _V2.y;
        }
        
        public static float DistanceSquared(Vector2 _A, Vector2 _B)
        {
            float num1 = _A.x - _B.x;
            float num2 = _A.y - _B.y;
            return num1 * num1 + num2 * num2;
        }

        public static float TriangleWave(float _V, float _Period, float _Amplitude)
        {
            float p = _Period;
            float a = _Amplitude;
            return Mathf.Abs((_V - 4 * p) % p - p * 0.5f) * 4f * a / p - a;
        }

        public static float SantimetersToInches(float _V)
        {
            return _V * 0.39370f;
        }
    }
}
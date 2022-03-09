using Common.Utils;
using UnityEngine;

namespace Common.Extensions
{
    public static class VectorExtensions
    {
        public static bool IsZero(this float _V)
        {
            return Mathf.Abs(_V) < MathUtils.Epsilon;
        }
        
        public static Vector4 SetX (this Vector4 _V, float _X) => new Vector4(_X, _V.y, _V.z, _V.w);
        public static Vector4 SetY (this Vector4 _V, float _Y) => new Vector4(_V.x, _Y, _V.z, _V.w);
        public static Vector4 SetZ (this Vector4 _V, float _Z) => new Vector4(_V.x, _V.y, _Z, _V.w);
        public static Vector4 SetW (this Vector4 _V, float _W) => new Vector4(_V.x, _V.y, _V.z, _W);
        
        public static Vector2 XY    (this Vector3 _V)                       => new Vector2(_V.x, _V.y);
        public static Vector3 SetX  (this Vector3 _V, float   _X)           => new Vector3(_X, _V.y, _V.z);
        public static Vector3 SetY  (this Vector3 _V, float   _Y)           => new Vector3(_V.x, _Y, _V.z);
        public static Vector3 SetZ  (this Vector3 _V, float   _Z)           => new Vector3(_V.x, _V.y, _Z);
        public static Vector3 PlusX (this Vector3 _V, float   _X)           => _V.SetX(_V.x + _X);
        public static Vector3 PlusY (this Vector3 _V, float   _Y)           => _V.SetY(_V.y + _Y);
        public static Vector3 PlusZ (this Vector3 _V, float   _Z)           => _V.SetZ(_V.z + _Z);
        public static Vector3 MinusX(this Vector3 _V, float   _X)           => _V.SetX(_V.x - _X);
        public static Vector3 MinusY(this Vector3 _V, float   _Y)           => _V.SetY(_V.y - _Y);
        public static Vector3 MinusZ(this Vector3 _V, float   _Z)           => _V.SetZ(_V.z - _Z);
        public static Vector3 SetXY (this Vector3 _V, Vector2 _XY)          => new Vector3(_XY.x, _XY.y, _V.z);
        public static Vector3 SetXY (this Vector3 _V, float   _X, float _Y) => new Vector3(_X, _Y, _V.z);
        
        public static Vector2 SetX  (this Vector2 _V, float _X) => new Vector2(_X, _V.y);
        public static Vector2 SetY  (this Vector2 _V, float _Y) => new Vector2(_V.x, _Y);
        public static Vector2 PlusX (this Vector2 _V, float _X) => _V.SetX(_V.x + _X);
        public static Vector2 PlusY (this Vector2 _V, float _Y) => _V.SetY(_V.y + _Y);
        public static Vector2 MinusX(this Vector2 _V, float _X) => _V.SetX(_V.x - _X);
        public static Vector2 MinusY(this Vector2 _V, float _Y) => _V.SetY(_V.y - _Y);
        
        public static float Angle2D(this Vector2 _V) => Vector2.Angle(_V, Vector2.right) * _V.y > 0 ? 1 : -1;
        public static Vector2Int ToVector2Int(this Vector2 _V) => new Vector2Int(Mathf.RoundToInt(_V.x), Mathf.RoundToInt(_V.y));
        public static Vector2 ToVector3(this Vector2 _V) => Vector3.zero.SetXY(_V);
        
        public static Vector2 Rotate(this Vector2 _V, float _Angle)
        {
            float sin = Mathf.Sin(_Angle);
            float cos = Mathf.Cos(_Angle);
            float tx = _V.x;
            float ty = _V.y;
            _V.x = cos * tx - sin * ty;
            _V.y = sin * tx + cos * ty;
            return _V;
        }
        
        public static Vector3 Abs(this Vector3 _V)
        {
            return new Vector3(Mathf.Abs(_V.x), Mathf.Abs(_V.y), Mathf.Abs(_V.z));
        }

        public static float Cross(this Vector2 _V1, Vector2 _V2)
        {
            return _V1.x * _V2.y - _V1.y * _V2.x;
        }
    }
}
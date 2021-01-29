using UnityEngine;

namespace Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 SetX(this Vector3 _V, float _X)
        {
            return new Vector3(_X, _V.y, _V.z);
        }
        
        public static Vector3 SetY(this Vector3 _V, float _Y)
        {
            return new Vector3(_V.x, _Y, _V.z);
        }
        
        public static Vector3 SetZ(this Vector3 _V, float _Z)
        {
            return new Vector3(_V.x, _V.y, _Z);
        }

        public static Vector3 SetXY(this Vector3 _V, float _Val)
        {
            return new Vector3(_Val, _Val, _V.z); 
        }

        public static Vector3 SetXY(this Vector3 _V, Vector2 _XY)
        {
            return new Vector3(_XY.x, _XY.y, _V.z);
        }
        
        public static Vector2 SetX(this Vector2 _V, float _X)
        {
            return new Vector2(_X, _V.y);
        }
        
        public static Vector2 SetY(this Vector2 _V, float _Y)
        {
            return new Vector2(_V.x, _Y);
        }

        public static Vector2 XY(this Vector3 _V)
        {
            return new Vector2(_V.x, _V.y);
        }
        
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
    }
}
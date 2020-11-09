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
    }
}
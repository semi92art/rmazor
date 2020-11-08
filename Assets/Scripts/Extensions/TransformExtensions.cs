using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        public static void SetPosX(this Transform _T, float _X)
        {
            var newPosition = _T.position.SetX(_X);
            _T.position = newPosition;
        }
        
        public static void SetPosY(this Transform _T, float _Y)
        {
            var newPosition = _T.position.SetY(_Y);
            _T.position = newPosition;
        }
        
        public static void SetPosZ(this Transform _T, float _Z)
        {
            var newPosition = _T.position.SetZ(_Z);
            _T.position = newPosition;
        }

        public static void SetPosXY(this Transform _T, float _X, float _Y)
        {
            var newPosition = _T.position.SetX(_X).SetY(_Y);
            _T.position = newPosition;
        }
    }
}
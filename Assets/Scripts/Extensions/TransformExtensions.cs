using UnityEngine;
using Utils;

namespace Extensions
{
    public static class TransformExtensions
    {
        #region api 
        
        public static void SetPosX(this Transform _T, float _X)
        {
            _T.position = _T.position.SetX(_X);
        }
        
        public static void SetLocalPosX(this Transform _T, float _X)
        {
            _T.localPosition = _T.localPosition.SetX(_X);
        }
        
        public static void SetPosY(this Transform _T, float _Y)
        {
            _T.position = _T.position.SetY(_Y);
        }
        
        public static void SetLocalPosY(this Transform _T, float _Y)
        {
            _T.localPosition = _T.localPosition.SetY(_Y);
        }
        
        public static void SetPosZ(this Transform _T, float _Z)
        {
            _T.position = _T.position.SetZ(_Z);
        }
        
        public static void SetLocalPosZ(this Transform _T, float _Z)
        {
            _T.localPosition = _T.localPosition.SetZ(_Z);
        }
        
        public static void SetPosXY(this Transform _T, float _X, float _Y)
        {
            var newPosition = _T.position.SetX(_X).SetY(_Y);
            _T.position = newPosition;
        }

        public static void SetLocalPosXY(this Transform _T, float _X, float _Y)
        {
            _T.localPosition = _T.localPosition.SetX(_X).SetY(_Y);
        }
        
        public static void LookAt2D(this Transform _T, Vector2 _To)
        {
            _T.eulerAngles = DirectionEulerAngles(_T.transform.position, _To);
        }
        
        #endregion
        
        #region nonpublic methods

        private static Vector3 DirectionEulerAngles(Vector2 _From, Vector2 _To)
        {
            return Vector3.forward * GeometryUtils.ZAngle(_From, _To);
        }
        
        #endregion
    }
}
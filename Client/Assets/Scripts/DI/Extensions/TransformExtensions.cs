using System.Linq;
using UnityEngine;
using Utils;

namespace DI.Extensions
{
    public static class TransformExtensions
    {
        #region api 
        
        public static void SetPosX        (this Transform _T, float _X) => _T.position = _T.position.SetX(_X);
        public static void PlusPosX       (this Transform _T, float _X) => _T.position = _T.position.PlusX(_X);
        public static void MinusPosX      (this Transform _T, float _X) => _T.position = _T.position.MinusX(_X);
        
        public static void SetPosY        (this Transform _T, float _Y) => _T.position = _T.position.SetY(_Y);
        public static void PlusPosY       (this Transform _T, float _Y) => _T.position = _T.position.PlusY(_Y);
        public static void MinusPosY      (this Transform _T, float _Y) => _T.position = _T.position.MinusY(_Y);
        
        public static void SetPosZ        (this Transform _T, float _Z) => _T.position = _T.position.SetZ(_Z);
        public static void PlusPosZ       (this Transform _T, float _Z) => _T.position = _T.position.PlusZ(_Z);
        public static void MinusPosZ      (this Transform _T, float _Z) => _T.position = _T.position.MinusZ(_Z);
        
        public static void SetPosXY       (this Transform _T, float _X, float _Y) => _T.position = _T.position.SetX(_X).SetY(_Y);
        public static void SetPosXY       (this Transform _T, Vector2 _XY) => _T.position = _T.position.SetX(_XY.x).SetY(_XY.y);
        public static void PlusPosXY      (this Transform _T, float _X, float _Y) => _T.position = _T.position.PlusX(_X).PlusY(_Y);
        public static void MinusPosXY     (this Transform _T, Vector2 _XY) => _T.position = _T.position.MinusX(_XY.x).MinusY(_XY.y);
        
        public static void SetLocalPosX   (this Transform _T, float _X) => _T.localPosition = _T.localPosition.SetX(_X);
        public static void PlusLocalPosX  (this Transform _T, float _X) => _T.localPosition = _T.localPosition.PlusX(_X);
        public static void MinusLocalPosX (this Transform _T, float _X) => _T.localPosition = _T.localPosition.MinusX(_X);
        
        public static void SetLocalPosY   (this Transform _T, float _Y) => _T.localPosition = _T.localPosition.SetY(_Y);
        public static void PlusLocalPosY  (this Transform _T, float _Y) => _T.localPosition = _T.localPosition.PlusY(_Y);
        public static void MinusLocalPosY (this Transform _T, float _Y) => _T.localPosition = _T.localPosition.MinusY(_Y);
        
        public static void SetLocalPosZ   (this Transform _T, float _Z) => _T.localPosition = _T.localPosition.SetZ(_Z);
        public static void PlusLocalPosZ  (this Transform _T, float _Z) => _T.localPosition = _T.localPosition.PlusZ(_Z);
        public static void MinusLocalPosZ (this Transform _T, float _Z) => _T.localPosition = _T.localPosition.MinusZ(_Z);
        
        public static void SetLocalPosXY  (this Transform _T, float _X, float _Y) =>_T.localPosition = _T.localPosition.SetX(_X).SetY(_Y);
        public static void SetLocalPosXY  (this Transform _T, Vector2 _XY) => _T.localPosition = _T.localPosition.SetXY(_XY);
        public static void PlusLocalPosXY (this Transform _T, float _X, float _Y) => _T.localPosition = _T.localPosition.PlusX(_X).PlusY(_Y);
        public static void PlusLocalPosXY (this Transform _T, Vector2 _XY) => _T.localPosition = _T.localPosition.PlusX(_XY.x).PlusY(_XY.y);
        public static void MinusLocalPosXY(this Transform _T, float _X, float _Y) => _T.localPosition = _T.localPosition.MinusX(_X).MinusY(_Y);
        public static void MinusLocalPosXY(this Transform _T, Vector2 _XY) => _T.localPosition = _T.localPosition.MinusX(_XY.x).MinusY(_XY.y);

        public static void LookAt2D(this Transform _T, Vector2 _To) => _T.eulerAngles = DirectionEulerAngles(_T.transform.position, _To);
        
        public static bool IsFullyVisibleFrom(this RectTransform _Item, RectTransform _Rect)
        {
            if (!_Item.gameObject.activeInHierarchy)
                return false;
            return _Item.CountCornersVisibleFrom(_Rect) == 4;
        }
        
        public static bool IsVisibleFrom(this RectTransform _Item, RectTransform _Rect)
        {
            if (!_Item.gameObject.activeInHierarchy)
                return false;
            return _Item.CountCornersVisibleFrom(_Rect) > 0;
        }
        
        #endregion
        
        #region nonpublic methods

        private static Vector3 DirectionEulerAngles(Vector2 _From, Vector2 _To) => Vector3.forward * GeometryUtils.ZAngle(_From, _To);
        
        private static int CountCornersVisibleFrom(this RectTransform _Item, RectTransform _Rect)
        {
            var itemCorners = new Vector3[4];
            _Item.GetWorldCorners(itemCorners);
            var rectCorners = new Vector3[4];
            _Rect.GetWorldCorners(rectCorners);
            var polygon = rectCorners.Select(_P => new Vector2(_P.x, _P.y)).ToArray();
            return itemCorners.Select(_Corner => new Vector2(_Corner.x, _Corner.y))
                .Count(_Point => GeometryUtils.IsPointInPolygon(polygon, _Point));
        }

        #endregion
    }
}
using UnityEngine;

namespace UICreationSystem
{
    public static class RectTransformExtensions
    {
        public static void SetLeft(this RectTransform _Item, float _Left)
        {
            _Item.offsetMin = new Vector2(_Left, _Item.offsetMin.y);
        }

        public static void SetRight(this RectTransform _Item, float _Right)
        {
            _Item.offsetMax = new Vector2(-_Right, _Item.offsetMax.y);
        }

        public static void SetTop(this RectTransform _Item, float _Top)
        {
            _Item.offsetMax = new Vector2(_Item.offsetMax.x, -_Top);
        }

        public static void SetBottom(this RectTransform _Item, float _Bottom)
        {
            _Item.offsetMin = new Vector2(_Item.offsetMin.x, _Bottom);
        }

        public static void Set(this RectTransform _Item,
            RectTransform _Parent,
            string _Name,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta)
        {
            _Item.SetParent(_Parent);
            _Item.name = _Name;
            _Item.Set(_Anchor, _AnchoredPosition, _Pivot, _SizeDelta);
        }

        public static void Set(this RectTransform _Item,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta)
        {
            _Item.anchorMin = _Anchor.Min;
            _Item.anchorMax = _Anchor.Max;
            _Item.anchoredPosition = _AnchoredPosition;
            _Item.pivot = _Pivot;
            _Item.sizeDelta = _SizeDelta;
            _Item.localScale = Vector3.one;
        }

        public static void Set(this RectTransform _Item, RectTransformLite _Lite)
        {
            if (_Lite.Anchor.HasValue)
            {
                _Item.anchorMin = _Lite.Anchor.Value.Min;
                _Item.anchorMax = _Lite.Anchor.Value.Max;
            }
            if (_Lite.AnchoredPosition.HasValue)
                _Item.anchoredPosition = _Lite.AnchoredPosition.Value;
            if (_Lite.Pivot.HasValue)
                _Item.pivot = _Lite.Pivot.Value;
            if (_Lite.SizeDelta.HasValue)
                _Item.sizeDelta = _Lite.SizeDelta.Value;
        }
    }
}

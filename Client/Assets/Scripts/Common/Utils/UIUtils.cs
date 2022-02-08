using Common.Entities.UI;
using Common.Extensions;
using Common.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Factories
{
    public static class UIUtils
    {
        #region public methods

        public static Canvas UiCanvas(
            string _Name,
            RenderMode _RenderMode,
            bool _PixelPerfect,
            int _SortOrder,
            AdditionalCanvasShaderChannels _AdditionalCanvasShaderChannels,
            CanvasScaler.ScaleMode _ScaleMode,
            Vector2Int _ReferenceResolution,
            CanvasScaler.ScreenMatchMode _ScreenMatchMode,
            float _Match,
            float _ReferencePixelsPerUnit,
            bool _IgnoreReversedGraphics,
            GraphicRaycaster.BlockingObjects _BlockingObjects
        )
        {
            var go = new GameObject(_Name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = _RenderMode;
            canvas.pixelPerfect = _PixelPerfect;
            canvas.sortingOrder = _SortOrder;
            canvas.additionalShaderChannels = _AdditionalCanvasShaderChannels;

            var canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = _ScaleMode;
            canvasScaler.referenceResolution = _ReferenceResolution;
            canvasScaler.screenMatchMode = _ScreenMatchMode;
            canvasScaler.matchWidthOrHeight = _Match;
            canvasScaler.referencePixelsPerUnit = _ReferencePixelsPerUnit;

            var graphicRaycaster = go.AddComponent<GraphicRaycaster>();
            graphicRaycaster.ignoreReversedGraphics = _IgnoreReversedGraphics;
            graphicRaycaster.blockingObjects = _BlockingObjects;

            return canvas;
        }
        
        public static RectTransform UiRectTransform(
            RectTransform _Parent,
            RectTransformLite _RtrLite)
        {
            return UiRectTransform(
                _Parent,
                default,
                _RtrLite.Anchor ?? default,
                _RtrLite.AnchoredPosition ?? default,
                _RtrLite.Pivot ?? default,
                _RtrLite.SizeDelta ?? default);
        }
        
        private static RectTransform UiRectTransform(
            RectTransform _Parent,
            string _Name,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta
        )
        {
            var item = new GameObject().AddComponent<RectTransform>();
            Set(item, _Parent, _Name, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);
            
#if UNITY_EDITOR
            item.gameObject.GetOrAddComponent<RectTransformHelper>();
#else
            item.gameObject.RemoveComponentIfExist<RectTransformHelper>();
#endif
            return item;
        }
        
        private static void Set(
            RectTransform _Item,
            RectTransform      _Parent,
            string             _Name,
            UiAnchor           _Anchor,
            Vector2            _AnchoredPosition,
            Vector2            _Pivot,
            Vector2            _SizeDelta)
        {
            _Item.SetParent(_Parent);
            _Item.name = _Name;
            Set(_Item, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);
        }

        private static void Set(
            RectTransform _Item,
            UiAnchor           _Anchor,
            Vector2            _AnchoredPosition,
            Vector2            _Pivot,
            Vector2            _SizeDelta)
        {
            _Item.anchorMin = _Anchor.Min;
            _Item.anchorMax = _Anchor.Max;
            _Item.anchoredPosition = _AnchoredPosition;
            _Item.pivot = _Pivot;
            _Item.sizeDelta = _SizeDelta;
            _Item.localScale = Vector3.one;
        }
        
        #endregion
    }
}


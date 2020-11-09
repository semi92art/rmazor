using Extensions;
using UI.Entities;
using UI.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Factories
{
    public class UiFactory
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
            GameObject go = new GameObject(_Name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = _RenderMode;
            canvas.pixelPerfect = _PixelPerfect;
            canvas.sortingOrder = _SortOrder;
            canvas.additionalShaderChannels = _AdditionalCanvasShaderChannels;

            CanvasScaler canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = _ScaleMode;
            canvasScaler.referenceResolution = _ReferenceResolution;
            canvasScaler.screenMatchMode = _ScreenMatchMode;
            canvasScaler.matchWidthOrHeight = _Match;
            canvasScaler.referencePixelsPerUnit = _ReferencePixelsPerUnit;

            GraphicRaycaster graphicRaycaster = go.AddComponent<GraphicRaycaster>();
            graphicRaycaster.ignoreReversedGraphics = _IgnoreReversedGraphics;
            graphicRaycaster.blockingObjects = _BlockingObjects;

            return canvas;
        }

        public static RectTransform UiRectTransform(
            RectTransform _Parent,
            string _Name,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta
        )
        {
            var item = new GameObject().AddComponent<RectTransform>();
            item.Set(_Parent, _Name, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);
            
#if UNITY_EDITOR
            item.gameObject.AddComponentIfNotExist<RectTransformHelper>();
#endif
            return item;
        }

        public static RectTransform UiRectTransform(
            RectTransform _Parent,
            string _Name,
            RectTransformLite _RtrLite)
        {
            return UiRectTransform(
                _Parent,
                _Name,
                _RtrLite.Anchor ?? default,
                 _RtrLite.AnchoredPosition ?? default,
                _RtrLite.Pivot ?? default,
                _RtrLite.SizeDelta ?? default);
        }

        public static RectTransform UiRectTransform(
            RectTransform _Parent,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta
        )
        {
            return UiRectTransform(
                _Parent,
                default,
                _Anchor,
                _AnchoredPosition,
                _Pivot,
                _SizeDelta);
        }
        
        public static RectTransform UiRectTransform(
            RectTransform _Parent,
            RectTransformLite _RtrLite)
        {
            return UiRectTransform(
                _Parent,
                default,
                _RtrLite);
        }
        
        public static RectTransform EmptyRectTransform => new RectTransform();

        public static void CopyRTransform(RectTransform _From, RectTransform _To)
        {
            _To.SetParent(_From.parent);
            _To.anchorMin = _From.anchorMin;
            _To.anchorMax = _From.anchorMax;
            _To.anchoredPosition = _From.anchoredPosition;
            _To.pivot = _From.pivot;
            _To.sizeDelta = _From.sizeDelta;
        }

        #endregion
    }
}


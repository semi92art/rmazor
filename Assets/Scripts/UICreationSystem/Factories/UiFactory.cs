using Clickers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UICreationSystem.Factories
{
    public class UiFactory
    {
        #region public methods

        public static Canvas UiCanvas(
            string _Id,
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
            GameObject go = new GameObject();
            go.name = _Id;
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

        public static Image UiImage(
            RectTransform _Item,
            string _StyleName)
        {
            Image image = _Item.gameObject.AddComponent<Image>();
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            if (style == null)
                return image;

            image.sprite = style.sprite;
            image.color = style.imageColor;
            image.raycastTarget = style.raycastImageTarget;
            image.useSpriteMesh = style.useSpriteMesh;
            image.preserveAspect = style.preserveAspect;
            image.pixelsPerUnitMultiplier = style.pixelsPerUnityMultyply;
            image.type = style.imageType;
            image.fillMethod = style.fillMethod;
            image.fillOrigin = style.fillOrigin;
            image.fillClockwise = style.fillClockwise;
            return image;
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
            var item = new GameObject(_Name).AddComponent<RectTransform>();
            item.Set(_Parent, _Name, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);
            
#if UNITY_EDITOR
            item.gameObject.AddComponentIfNotExist<RectTranshormHelper>();
#endif
            return item;
        }

        public static void CopyRTransform(RectTransform _From, RectTransform _To)
        {
            _To.parent = _From.parent;
            _To.anchorMin = _From.anchorMin;
            _To.anchorMax = _From.anchorMax;
            _To.anchoredPosition = _From.anchoredPosition;
            _To.pivot = _From.pivot;
            _To.sizeDelta = _From.sizeDelta;
        }

        public static void CopyTransform(Transform _From, Transform _To)
        {
            _To.position = _From.position;
            _To.rotation = _From.rotation;
            _To.localScale = _From.localScale;
        }

        #endregion
    }
}


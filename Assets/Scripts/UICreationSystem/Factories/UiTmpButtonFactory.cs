using Clickers.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace UICreationSystem.Factories
{
    public static class UiTmpButtonFactory
    {
        public static Button Create(
            RectTransform _Parent,
            string _Name,
            UIAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta,
            string _StyleName,
            UnityEngine.Events.UnityAction _OnClick)
        {
            RectTransform rTr = UiFactory.UiRectTransform(
                _Parent,
                _Name,
                _Anchor,
                _AnchoredPosition,
                _Pivot,
                _SizeDelta);
            
            rTr.SetParent(_Parent);
            rTr.anchorMin = _Anchor.Min;
            rTr.anchorMax = _Anchor.Max;
            rTr.anchoredPosition = _AnchoredPosition;
            rTr.pivot = _Pivot;
            rTr.sizeDelta = _SizeDelta;
            rTr.localScale = Vector3.one;
            
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            Image image = rTr.gameObject.AddComponent<Image>();
            image.GetCopyOf(style.button.GetComponent<Image>());
            Button button = rTr.gameObject.AddComponent<Button>();
            button.GetCopyOf(style.button.GetComponent<Button>());
            
            button.targetGraphic = image;
            var @event = new Button.ButtonClickedEvent();
            @event.AddListener(_OnClick);
            button.onClick = @event;

            return button;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

static class UICreatorText
{
    public static RectTransform Create(RectTransform _parent, string _name, UIAnchor _anchor, Vector2 _anchoredPosition, Vector2 _pivot, Vector2 _SizeDelta, string _styleName)
    {
        return UIFactory.UIText(
         UIFactory.UIRectTransform(
             _parent,
             _name,
             _anchor,
             _anchoredPosition,
             _pivot,
             _SizeDelta),
         _styleName).rectTransform;
    }
}


using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;


static class UICreatorImage
{
    public static RectTransform Create(RectTransform _parent, string _name, UiAnchor _anchor, Vector2 _anchoredPosition, Vector2 _pivot, Vector2 _SizeDelta, string _styleName)
    {
        return UiFactory.UiImage(
         UiFactory.UiRectTransform(
             _parent,
             _name,
             _anchor,
             _anchoredPosition,
             _pivot,
             _SizeDelta),
         _styleName).rectTransform;
    }
}


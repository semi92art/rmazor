using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;

static class UICreatorText
{
    public static RectTransform Create(
        RectTransform _Parent,
        string _Name,
        UIAnchor _Anchor,
        Vector2 _AnchoredPosition, 
        Vector2 _Pivot,
        Vector2 _SizeDelta,
        string _StyleName)
    {
        return UiFactory.UiText(
         UiFactory.UiRectTransform(
             _Parent,
             _Name,
             _Anchor,
             _AnchoredPosition,
             _Pivot,
             _SizeDelta),
         _StyleName).rectTransform;
    }
}


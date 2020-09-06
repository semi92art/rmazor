using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;

static class UICreatorButton
{
    public static RectTransform Create(
        RectTransform _Parent, 
        string _Name,
        UIAnchor _Anchor,
        Vector2 _AnchoredPosition, 
        Vector2 _Pivot,
        Vector2 _SizeDelta,
        string _StyleName,
        UnityEngine.Events.UnityAction _Action,
        Image _TargetGraphic)
    {
        return UiFactory.UiButton(
            UiFactory.UiRectTransform(
                _Parent,
                _Name,
                _Anchor,
                _AnchoredPosition,
               _Pivot,
               _SizeDelta),
            _StyleName,
            _Action,
            _TargetGraphic).RTransform();
    }
}


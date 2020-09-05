using UnityEngine;
using UnityEngine.UI;

static class UICreatorButton
{
    public static RectTransform Create(RectTransform _parent, string _name, UIAnchor _anchor, Vector2 _anchoredPosition, Vector2 _pivot, Vector2 _SizeDelta, string _styleName, UnityEngine.Events.UnityAction _action, Image _targetGraphic)
    {
        return UIFactory.UIButton(
            UIFactory.UIRectTransform(
                _parent,
                _name,
                _anchor,
                _anchoredPosition,
               _pivot,
               _SizeDelta),
            _styleName,
            _action,
            _targetGraphic).rectTransform();
    }
}


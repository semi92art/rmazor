using UnityEngine;
using UnityEngine.EventSystems;

public static class UiExtentions
{
    public static RectTransform RTransform(this Canvas _Canvas) =>
        _Canvas.GetComponent<RectTransform>();
    
    public static RectTransform RTransform(this UIBehaviour _UiBehaviour) =>
        _UiBehaviour.GetComponent<RectTransform>();

    public static RectTransform RTransform(this RectTranshormHelper _Helper) =>
        _Helper.GetComponent<RectTransform>();
    
    public static RectTransform RTransform(this GameObject _Object) =>
        _Object.GetComponent<RectTransform>();

    public static void SetParent(this UIBehaviour _UiBehaviour, UIBehaviour _NewParent)
    {
        _UiBehaviour.SetParent(_NewParent.RTransform());
    }

    public static void SetParent(this UIBehaviour _UiBehaviour, RectTransform _NewParent)
    {
        _UiBehaviour.RTransform().SetParent(_NewParent);
    }
}
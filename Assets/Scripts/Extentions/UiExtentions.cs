using UICreationSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extentions
{
    public static class UiExtentions
    {
        public static RectTransform RTransform(this Canvas _Canvas) =>
            _Canvas.GetComponent<RectTransform>();
    
        public static RectTransform RTransform(this UIBehaviour _UiBehaviour) =>
            _UiBehaviour.GetComponent<RectTransform>();

        public static RectTransform RTransform(this RectTransformHelper _Helper) =>
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

        public static void SetOnClick(this Button _Button, UnityEngine.Events.UnityAction _OnClick)
        {
            var @event = new Button.ButtonClickedEvent();
            @event.AddListener(_OnClick);
            _Button.onClick = @event;
        }
    }
}
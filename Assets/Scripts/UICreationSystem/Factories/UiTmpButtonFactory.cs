using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UICreationSystem.Factories
{
    public static class UiTmpButtonFactory
    {
        public static GameObject Create(
            RectTransform _Parent,
            string _Name,
            string _Text,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta,
            string _StyleName,
            UnityEngine.Events.UnityAction _OnClick
            )
        {

            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            GameObject buttonObj = Object.Instantiate(style.button);

            var rTr = buttonObj.RTransform();
            rTr.Set(_Parent, _Name, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);
            
            TextMeshProUGUI buttonObjText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonObjText.text = _Text;

            Button button = buttonObj.GetComponentInChildren<Button>();

            var iconObj = buttonObj.transform.GetChild(1);
            if (style.sprite != null)
            {
                iconObj.GetComponent<RectTransform>().SetRight(280);
                iconObj.transform.localScale = new Vector3(0.7f,0.7f,1);
                var tmp = iconObj.GetComponent<RectTranshormHelper>();
                Image icon = iconObj.GetComponent<Image>();
                icon.sprite = style.sprite;
                icon.color = style.imageColor;
            }

            var @event = new Button.ButtonClickedEvent();
            @event.AddListener(_OnClick);
            button.onClick = @event;

            return buttonObj;
        }
    }
}
using Clickers.Utils;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UICreationSystem.Factories
{
    public static class UiTmpInputFieldFactory
    {
        public static TMP_InputField Create(
            RectTransform _Parent,
            string _Name,
            UIAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta,
            string _StyleName,
            Image _TargetGraphic,
            TextMeshProUGUI _TargetText,
            TextMeshProUGUI _TargetPlaceHolder
        )
        {
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            GameObject obj = Object.Instantiate(style.inputField);
            
            var rTr = obj.RTransform();
            rTr = UiFactory.UiRectTransform(
                _Parent,
                _Name,
                _Anchor,
                _AnchoredPosition,
                _Pivot,
                _SizeDelta,
                rTr);

            var inputField = rTr.GetComponent<TMP_InputField>();
            
            inputField.placeholder = _TargetPlaceHolder;
            _TargetPlaceHolder.SetParent(rTr);
            inputField.textComponent = _TargetText;
            _TargetText.SetParent(rTr);
            inputField.targetGraphic = _TargetGraphic;
            _TargetGraphic.SetParent(rTr);
            
            return inputField;
        }
        
    }
}
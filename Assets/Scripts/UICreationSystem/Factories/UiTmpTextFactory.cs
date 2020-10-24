using UnityEngine;
using TMPro;

namespace UICreationSystem.Factories
{
    public static class UiTmpTextFactory
    {
        public static TextMeshProUGUI Create(
            RectTransform _Parent,
            string _Name,
            UiAnchor _Anchor,
            Vector2 _AnchoredPosition,
            Vector2 _Pivot,
            Vector2 _SizeDelta,
            string _StyleName,
            string _Text)
        {
            UIStyleObject style = ResLoader.GetStyle(_StyleName);
            GameObject obj = Object.Instantiate(style.text);
            
            var rTr = obj.RTransform();
            rTr.Set(_Parent, _Name, _Anchor, _AnchoredPosition, _Pivot, _SizeDelta);

            TextMeshProUGUI result = obj.GetComponent<TextMeshProUGUI>();
            result.text = _Text;
            return result;
        }
    }
}
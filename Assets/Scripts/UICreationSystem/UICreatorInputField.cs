using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;


static class UICreatorInputField
{
    public static InputField Create(
        RectTransform _Parent, 
        string _Name, 
        UIAnchor _Anchor,
        Vector2 _AnchoredPosition, 
        Vector2 _Pivot,
        Vector2 _SizeDelta,
        string _StyleName,
        Image _TargetGraphic,
        Text _TargetText,
        Text _TargetPlaceholder)
    {
        return Create(
         UiFactory.UiRectTransform(
             _Parent,
             _Name,
             _Anchor,
             _AnchoredPosition,
             _Pivot,
             _SizeDelta),
         _StyleName,
         _TargetGraphic,
         _TargetText,
         _TargetPlaceholder);
    }
    
    private static InputField Create(
        RectTransform _Item,
        string _StyleName,
        Image _TargetGraphic,
        Text _TargetText,
        Text _TargetPlaceholder)
    {
        InputField inputField = _Item.gameObject.AddComponent<InputField>();
        UIStyleObject style = ResLoader.GetStyle(_StyleName);

        inputField.placeholder = _TargetPlaceholder;
        _TargetPlaceholder.SetParent(inputField);
        inputField.textComponent = _TargetText;
        _TargetText.SetParent(inputField);
        inputField.targetGraphic = _TargetGraphic;
        _TargetGraphic.SetParent(inputField);

        return inputField;
    }
}


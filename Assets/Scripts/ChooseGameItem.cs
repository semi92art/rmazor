using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UICreationSystem;

public class ChooseGameItem : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI title;
    public Image icon;
    public TextMeshProUGUI comingSoonLabel;
    
    public void Init(ChooseGameItemProps _Props)
    {
        title.text = _Props.Title;
        icon.sprite = _Props.Icon;
        button.interactable = !_Props.IsComingSoon;
        button.SetOnClick(() => _Props.OnClick?.Invoke());
        gameObject.SetActive(_Props.IsVisible);
        SetComingSoon(_Props.IsComingSoon);
    }
    
    private void SetComingSoon(bool _IsComingSoon)
    {
        comingSoonLabel.enabled = _IsComingSoon;
        if (_IsComingSoon & ColorUtility.TryParseHtmlString("#B7B7B7", out Color disabledColor))
        {
            icon.color = disabledColor;
            title.color = disabledColor;
        }
    }
}

public class ChooseGameItemProps
{
    public string Title { get; set; }
    public Sprite Icon { get; set; }
    public bool IsComingSoon { get; set; }
    public bool IsVisible { get; set; }
    public UnityEngine.Events.UnityAction OnClick { get; set; }
}
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelItems
{
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
            button.SetOnClick(() => _Props.Click?.Invoke());
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
        public int GameId { get; }
        public Sprite Icon { get; }
        public string Title { get; }
        public bool IsComingSoon { get; }
        public bool IsVisible { get; }
        public UnityEngine.Events.UnityAction Click { get; set; }

        public ChooseGameItemProps(
            int _GameId,
            Sprite _Icon,
            string _Title,
            bool _IsComingSoon,
            bool _IsVisible)
        {
            GameId = _GameId;
            Icon = _Icon;
            Title = _Title;
            IsComingSoon = _IsComingSoon;
            IsVisible = _IsVisible;
        }
    }
}
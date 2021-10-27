using Entities;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemAction : SimpleUiDialogItemView
    {
        [SerializeField] private Button button;
        public TextMeshProUGUI title;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Select)
        {
            InitCore(_Managers, _UITicker);
            name = "Setting";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Select);
        }
    }
}
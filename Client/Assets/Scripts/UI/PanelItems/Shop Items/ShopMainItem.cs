using Entities;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMainItem : SimpleUiDialogItemView
    {
        [SerializeField] private Button button;
        public Image icon;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            UnityAction _Select,
            Sprite _Icon)
        {
            InitCore(_Managers, _UITicker);
            name = "Shop Item";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Select);
            icon.sprite = _Icon;
        }
    }
}
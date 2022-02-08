using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Managers;
using RMAZOR.Views.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMainItem : ShopItemBase
    {
        [SerializeField] private Button button;
        
        public override void Init(
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            Init(_AudioManager, _UITicker, _ColorProvider);
            name = "Shop Item";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
        }
    }
}
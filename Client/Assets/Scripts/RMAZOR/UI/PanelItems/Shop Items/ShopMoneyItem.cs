using Common.Managers;
using Common.Providers;
using Common.Ticker;
using UnityEngine.Events;

namespace UI.PanelItems.Shop_Items
{
    public class ShopMoneyItem : ShopItemBase
    {
        public override void Init(
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            base.Init(_AudioManager, _LocalizationManager, _UITicker, _ColorProvider, _Click, _Info);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Reward > 0)
                title.text = _Info.Reward.ToString();
        }
    }
}
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using UnityEngine.Events;

namespace RMAZOR.UI.PanelItems.Shop_Items
{
    public class ShopMoneyItem : ShopItemBase
    {
        public override void Init(
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            base.Init(_UITicker, _ColorProvider, _AudioManager, _LocalizationManager, _PrefabSetManager, _Click, _Info);
            itemIcon.sprite = _Info.Icon;
            if (_Info.Reward > 0)
                title.text = _Info.Reward.ToString();
        }
    }
}
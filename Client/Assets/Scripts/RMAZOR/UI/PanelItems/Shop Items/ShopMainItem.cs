using Common.Managers;
using Common.Providers;
using Common.Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Items
{
    public class ShopMainItem : ShopItemBase
    {
        [SerializeField] private Button button;
        
        public override void Init(
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            Init(_UITicker, _ColorProvider, _AudioManager, _LocalizationManager, _PrefabSetManager);
            name = "Shop Item";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
        }
    }
}
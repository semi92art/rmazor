using Common.Managers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ShopMainItem : ShopItemBase
    {
        [SerializeField] private Button button;
        
        public override void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            Init(_UITicker, _AudioManager, _LocalizationManager);
            name = "Shop Item";
            button.onClick.AddListener(PlayButtonClickSound);
            button.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
        }
    }
}
﻿using Common.Managers;
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
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            Init(_UITicker, _AudioManager, _LocalizationManager);
            name = "Shop Item";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Click);
            itemIcon.sprite = _Info.Icon;
        }
    }
}
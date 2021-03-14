using System;
using System.Collections.Generic;
using Constants;
using Entities;
using GameHelpers;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Extensions;
using Zenject;


namespace UI.PanelItems
{
    public class ShopItemMoney : ShopItemBase, IShopItem
    {
        [SerializeField] private Image firstCurrencyIcon;
        [SerializeField] private Image secondCurrencyIcon;

        public static IShopItem Create(RectTransform _Parent) =>
            ShopItemBase.Create<ShopItemMoney>(_Parent, "shop_item_money");
        
        private IBankManager BankManager { get; set; }

        [Inject, Obsolete("For internal use")]
        public void Inject(IBankManager _BankManager) => BankManager = _BankManager; 

        public void Init(ShopItemProps _Props, IEnumerable<GameObserver> _Observers)
        {
            var rewards = _Props.Rewards;
            description.text = rewards[BankItemType.FirstCurrency].ToNumeric() + " " + "gold" + "\n" + 
                               rewards[BankItemType.SecondCurrency].ToNumeric() + " " + "diamonds";
            firstCurrencyIcon.sprite = PrefabUtilsEx.GetObject<Sprite>(
                "icons_bags", $"icon_gold_bag_{BagSize(_Props.Size)}");
            secondCurrencyIcon.sprite = PrefabUtilsEx.GetObject<Sprite>(
                "icons_bags", $"icon_diamonds_bag_{BagSize(_Props.Size)}");
            
            UnityAction action = () =>
            {
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                Notifyer.RaiseNotify(
                    this,
                    CommonNotifyMessages.PurchaseCommand,
                    _Props,
                    (UnityAction) (() => BankManager.PlusBankItems(_Props.Rewards)));
            };
            
            base.Init(action, _Props, _Observers);
        }
    }
}
using System.Collections.Generic;
using Constants;
using Entities;
using GameHelpers;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems
{
    public class ShopItemMoney : ShopItemBase, IShopItem
    {
        [SerializeField] private Image goldIcon;
        [SerializeField] private Image diamondIcon;

        public static IShopItem Create(RectTransform _Parent) =>
            ShopItemBase.Create<ShopItemMoney>(_Parent, "shop_item_money");

        public void Init(ShopItemProps _Props, IEnumerable<GameObserver> _Observers)
        {
            var rewards = _Props.Rewards;
            description.text = rewards[MoneyType.Gold].ToNumeric() + " " + "gold" + "\n" + 
                               rewards[MoneyType.Diamonds].ToNumeric() + " " + "diamonds";
            goldIcon.sprite = PrefabInitializer.GetObject<Sprite>(
                "icons_bags", $"icon_gold_bag_{BagSize(_Props.Size)}");
            diamondIcon.sprite = PrefabInitializer.GetObject<Sprite>(
                "icons_bags", $"icon_diamonds_bag_{BagSize(_Props.Size)}");
            
            UnityAction action = () =>
            {
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                Notifyer.RaiseNotify(
                    this,
                    CommonNotifyMessages.PurchaseCommand,
                    _Props,
                    (UnityAction) (() => MoneyManager.Instance.PlusMoney(_Props.Rewards)));
            };
            
            base.Init(action, _Props, _Observers);
        }
    }
}
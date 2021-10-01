using Constants;

using DI.Extensions;
using Entities;
using GameHelpers;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class ShopItemMoney : ShopItemBase, IShopItem
    {
        [SerializeField] private Image firstCurrencyIcon;
        [SerializeField] private Image secondCurrencyIcon;

        public static IShopItem Create(RectTransform _Parent) =>
            Create<ShopItemMoney>(_Parent, "shop_item_money");

        public void Init(
            ShopItemProps _Props,
            IManagersGetter _Managers)
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
                Managers.Notify(
                    _SM =>
                    {
                        _SM.PlayClip(AudioClipNames.UIButtonClick);
                    },
                    _OnPurchasesManager: _PM =>
                    {
                        _PM.Purchase(_Props.PurchaseCode, 
                            () => BankManager.Instance.PlusBankItems(_Props.Rewards));
                    });
   
            };
            
            base.Init(action, _Props, _Managers);
        }
    }
}
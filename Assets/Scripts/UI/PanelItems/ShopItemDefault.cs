using System.Collections.Generic;
using Constants;
using Entities;
using Exceptions;
using GameHelpers;
using Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI.PanelItems
{
    public class ShopItemDefault : ShopItemBase, IShopItem
    {
        [SerializeField] private Image icon;

        public static IShopItem Create(RectTransform _Parent) =>
            ShopItemBase.Create<ShopItemDefault>(_Parent, "shop_item_default");
        
        public void Init(ShopItemProps _Props, IEnumerable<GameObserver> _Observers)
        {
            UnityAction afterPurchaseAction;
            switch (_Props.Type)
            {
                case ShopItemType.NoAds:
                    description.text = _Props.Description;
                    icon.sprite = PrefabInitializer.GetObject<Sprite>("icons", "icon_no_ads");
                    afterPurchaseAction = () => AdsManager.Instance.ShowAds = false;
                    break;
                case ShopItemType.Lifes:
                    description.text = _Props.Rewards[MoneyType.Lifes].ToNumeric() + " " + "lifes";
                    icon.sprite = PrefabInitializer.GetObject<Sprite>(
                        "icons_bags", $"icon_lifes_bag_{BagSize(_Props.Size)}");
                    afterPurchaseAction = () => MoneyManager.Instance.PlusMoney(_Props.Rewards);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Props.Type);
            }
            
            UnityAction action = () =>
            {
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                Notifyer.RaiseNotify(
                    this,
                    CommonNotifyMessages.PurchaseCommand,
                    _Props,
                    afterPurchaseAction);
            };
            base.Init(action, _Props, _Observers);
        }
    }
}
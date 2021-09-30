using System.Collections.Generic;
using Constants;
using Constants.NotifyMessages;
using Entities;
using Exceptions;
using GameHelpers;
using Managers;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class ShopItemDefault : ShopItemBase, IShopItem
    {
        [SerializeField] private Image icon;

        public static IShopItem Create(RectTransform _Parent) =>
            Create<ShopItemDefault>(_Parent, "shop_item_default");
        
        public void Init(
            ShopItemProps _Props,
            IGameObservable _GameObservable)
        {
            UnityAction afterPurchaseAction;
            switch (_Props.Type)
            {
                case ShopItemType.NoAds:
                    description.text = _Props.Description;
                    icon.sprite = PrefabUtilsEx.GetObject<Sprite>("icons", "icon_no_ads");
                    afterPurchaseAction = () => AdsManager.Instance.ShowAds = false;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Props.Type);
            }
            
            UnityAction action = () =>
            {
                GameObservable.Notify(SoundNotifyMessages.PlayAudioClip, AudioClipNames.UIButtonClick);
                GameObservable.Notify(CommonNotifyMessages.PurchaseCommand, _Props, afterPurchaseAction);
            };
            base.Init(action, _Props, _GameObservable);
        }
    }
}
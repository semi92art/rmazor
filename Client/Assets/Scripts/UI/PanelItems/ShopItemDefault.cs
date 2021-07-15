﻿using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Exceptions;
using GameHelpers;
using Managers;
using Network;
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
                    icon.sprite = PrefabUtilsEx.GetObject<Sprite>("icons", "icon_no_ads");
                    afterPurchaseAction = () => AdsManager.Instance.ShowAds = false;
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
﻿using System.Collections.Generic;
using Constants;
using DI.Extensions;
using Entities;
using UnityEngine.Events;
using Utils;

namespace Managers.IAP
{
    public class ShopManagerFake : ShopManagerBase
    {
        private const EShopProductResult Result = EShopProductResult.Fail;
        
        private readonly Dictionary<int, ShopItemArgs> m_ShopItems = new Dictionary<int, ShopItemArgs>
        {
            {PurchaseKeys.Money1, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => Result
            }},
            {PurchaseKeys.Money2, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "200",
                Result = () => Result
            }},
            {PurchaseKeys.Money3, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "300",
                Result = () => Result
            }},
            {PurchaseKeys.NoAds, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => Result
            }}
        };
        
        private readonly   Dictionary<int, UnityAction> m_PurchaseActions = new Dictionary<int, UnityAction>();

        public override void RestorePurchases()
        {
            SaveUtils.PutValue(SaveKeys.DisableAds, null);
            Dbg.Log("Purchases restored.");
        }

        public override void Purchase(int _Key)
        {
            m_PurchaseActions[_Key]?.Invoke();
        }

        public override void RateGame(bool _JustSuggest = true)
        {
            // do nothing
        }

        public override ShopItemArgs GetItemInfo(int _Key)
        {
            var args = m_ShopItems[_Key];
            return args;
        }

        public override void SetPurchaseAction(int _Key, UnityAction _Action)
        {
            m_PurchaseActions.SetSafe(_Key, _Action);
        }

        public override void SetDeferredAction(int _Key, UnityAction _Action)
        {
            // do nothing
        }
    }
}
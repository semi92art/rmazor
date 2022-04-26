﻿using System.Collections.Generic;
using UnityEngine.Events;

namespace Common.Managers.IAP
{
    public interface IShopManager : IInit
    {
        void         RegisterProductInfos(List<ProductInfo> _Products);
        void         RestorePurchases();
        void         Purchase(int _Key);
        bool         RateGame(bool _JustSuggest);
        ShopItemArgs GetItemInfo(int _Key);
        void         SetPurchaseAction(int _Key, UnityAction _Action);
        void         SetDeferredAction(int _Key, UnityAction _Action);
    }
}
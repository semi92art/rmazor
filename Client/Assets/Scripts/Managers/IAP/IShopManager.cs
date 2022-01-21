using System.Collections.Generic;
using Common;
using UnityEngine.Events;

namespace Managers.IAP
{
    public interface IShopManager : IInit
    {
        void         RegisterProductInfos(List<ProductInfo> _Products);
        void         RestorePurchases();
        void         Purchase(int _Key);
        void         RateGame(bool _JustSuggest = true);
        ShopItemArgs GetItemInfo(int _Key);
        void         SetPurchaseAction(int _Key, UnityAction _Action);
        void         SetDeferredAction(int _Key, UnityAction _Action);
    }
}
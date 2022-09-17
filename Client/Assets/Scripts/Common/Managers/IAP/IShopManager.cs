using System.Collections.Generic;
using UnityEngine.Events;

namespace Common.Managers.IAP
{
    public interface IShopManager : IInit
    {
        void         RegisterProductInfos(List<ProductInfo> _Products);
        void         RestorePurchases();
        void         Purchase(int _Key);
        bool         RateGame();
        ShopItemArgs GetItemInfo(int _Key);
        void         AddPurchaseAction(int _ProductKey, int _ActionKey, UnityAction _Action);
        void         AddDeferredAction(int _ProductKey, int _ActionKey, UnityAction _Action);
    }
}
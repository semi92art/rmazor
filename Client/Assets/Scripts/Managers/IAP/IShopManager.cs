using UnityEngine.Events;

namespace Managers.IAP
{
    public interface IShopManager : IInit
    {
        void         RestorePurchases();
        void         Purchase(int _Key);
        void         RateGame();
        ShopItemArgs GetItemInfo(int _Key);
        void         SetPurchaseAction(int _Key, UnityAction _Action);
        void         SetDeferredAction(int _Key, UnityAction _Action);
    }
}
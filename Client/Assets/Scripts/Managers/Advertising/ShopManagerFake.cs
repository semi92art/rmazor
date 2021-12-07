using System.Collections.Generic;
using Constants;
using Entities;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public class ShopManagerFake : IShopManager
    {
        private readonly Dictionary<int, ShopItemArgs> m_ShopItems = new Dictionary<int, ShopItemArgs>
        {
            {PurchaseKeys.Money1, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => EShopProductResult.Success,
                HasReceipt = false
            }},
            {PurchaseKeys.Money2, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "200",
                Result = () => EShopProductResult.Success,
                HasReceipt = false
            }},
            {PurchaseKeys.Money3, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "300",
                Result = () => EShopProductResult.Success,
                HasReceipt = false
            }},
            {PurchaseKeys.NoAds, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => EShopProductResult.Success,
                HasReceipt = false
            }},
        };
        
        public event UnityAction Initialized;
        
        public void Init()
        {
            Initialized?.Invoke();
        }

        public void RestorePurchases()
        {
            SaveUtils.PutValue(SaveKeys.DisableAds, null);
            Dbg.Log("Purchases restored.");
        }

        public void Purchase(int _Key, UnityAction _OnPurchase)
        {
            _OnPurchase?.Invoke();
        }

        public void RateGame()
        {
            Dbg.Log($"{nameof(RateGame)} not available in editor.");
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var args = m_ShopItems[_Key];
            return args;
        }
    }
}
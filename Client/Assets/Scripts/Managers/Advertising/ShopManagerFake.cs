using System.Collections.Generic;
using Constants;
using Entities;
using UnityEditor;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public class ShopManagerFake : IShopManager
    {
        private const EShopProductResult Result = EShopProductResult.Fail;
        
        private readonly Dictionary<int, ShopItemArgs> m_ShopItems = new Dictionary<int, ShopItemArgs>
        {
            {PurchaseKeys.Money1, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "100",
                Result = () => Result,
                HasReceipt = false
            }},
            {PurchaseKeys.Money2, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "200",
                Result = () => Result,
                HasReceipt = false
            }},
            {PurchaseKeys.Money3, new ShopItemArgs
            {
                Currency = "RUB",
                Price = "300",
                Result = () => Result,
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
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Dialog", "Available only on device", "OK");
#endif
            }
            _OnPurchase?.Invoke();
        }

        public void RateGame()
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog("Dialog", "Available only on device", "OK");
#endif
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var args = m_ShopItems[_Key];
            return args;
        }
    }
}
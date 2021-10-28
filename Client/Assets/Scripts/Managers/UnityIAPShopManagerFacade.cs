using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using Utils;

namespace Managers
{
    public enum EShopProductResult
    {
        Pending,
        Success,
        Fail
    }
    
    public class ShopItemArgs : EventArgs
    {
        public string Price { get; set; }
        public string Currency { get; set; }
        public Func<EShopProductResult> Result { get; set; }
    }
    
    public interface IShopManager : IInit
    {
        void RestorePurchases();
        void Purchase(int _Key, UnityAction _OnPurchase);
        void GoToRatePage();
        ShopItemArgs GetItemInfo(int _Key);
    }
    
    public class UnityIAPShopManagerFacade : UnityIAPShopManager, IShopManager
    {
        // Product identifiers for all products capable of being purchased: 
        // "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
        // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
        // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

        // General product identifiers for the consumable, non-consumable, and subscription products.
        // Use these handles in the code to reference which product to purchase. Also use these values 
        // when defining the Product Identifiers on the store. Except, for illustration purposes, the 
        // kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
        // specific mapping to Unity Purchasing's AddProduct, below.

        protected override List<ProductInfo> Products => new List<ProductInfo>
        {
            new ProductInfo(1, "money_1", ProductType.Consumable),
            new ProductInfo(2, "money_4", ProductType.Consumable),
            new ProductInfo(3, "money_3", ProductType.Consumable),
            new ProductInfo(4, "money_2", ProductType.Consumable)
        };
        
        private readonly Dictionary<string, UnityAction> m_OnPurchaseActions =
            new Dictionary<string, UnityAction>();

        #region api

        public event UnityAction Initialized;
        
        public void Init()
        {
            InitializePurchasing();
            Coroutines.Run(Coroutines.WaitWhile(
                () =>  m_StoreController == null || m_StoreExtensionProvider == null,
                () => Initialized?.Invoke()));
            
        }
        
        public void Purchase(int _Key, UnityAction _OnPurchase)
        {
            string id = GetProductId(_Key);
            if (m_OnPurchaseActions.ContainsKey(id))
                m_OnPurchaseActions[id] = _OnPurchase;
            else
                m_OnPurchaseActions.Add(id, _OnPurchase);
            BuyProductID(id);
        }

        public void GoToRatePage()
        {
            throw new System.NotImplementedException();
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var res = new ShopItemArgs();
            GetProductItemInfo(_Key, ref res);
            return res;
        }

        public override void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Dbg.LogWarning("RestorePurchases FAIL. Not initialized.");
                return;
            }
            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer || 
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Dbg.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions(_Result => {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Dbg.Log("RestorePurchases continuing: " + _Result + ". If no further messages, no purchases available to restore.");
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Dbg.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public override PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs _Args)
        {
            bool purchaseActionFound = false;
            foreach (var kvp in m_OnPurchaseActions.ToList())
            {
                // A consumable product has been purchased by this user.
                if (!string.Equals(_Args.purchasedProduct.definition.id, kvp.Key, StringComparison.Ordinal)) 
                    continue;
                purchaseActionFound = true;
                Dbg.Log($"ProcessPurchase: PASS. Product: '{_Args.purchasedProduct.definition.id}'");
                kvp.Value?.Invoke();
                m_OnPurchaseActions.Remove(kvp.Key);
                break;
            }
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            if (!purchaseActionFound) 
            {
                Dbg.Log("ProcessPurchase: FAIL. " +
                        $"Unrecognized product: '{_Args.purchasedProduct.definition.id}'");
            }
            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Complete;
        }

        #endregion
        
        #region nonpublic methods

        private string GetProductId(int _Key)
        {
            var product = Products.FirstOrDefault(_P => _P.Key == _Key);
            if (product != null) 
                return product.Id;
            Dbg.LogError($"Get Product Id failed. Product with key {_Key} does not exist");
            return string.Empty;
        }

        private void GetProductItemInfo(int _Key, ref ShopItemArgs _Args)
        {
            var shopProductResult = EShopProductResult.Pending;
            _Args.Result = () => shopProductResult;
            if (!IsInitialized())
            {
                Dbg.LogWarning("Get Product Item Info Failed. Not initialized.");
                shopProductResult = EShopProductResult.Fail;
                return;
            }
            string id = GetProductId(_Key);
            var product = m_StoreController.products.set.FirstOrDefault(_P => _P.definition.id == id);
            if (product == null)
            {
                Dbg.LogWarning($"Get Product Item Info Failed. Product with id {id} does not exist");
                shopProductResult = EShopProductResult.Fail;
                return;
            }
            _Args.Price = product.metadata.localizedPriceString;
            shopProductResult = EShopProductResult.Success;
        }

        #endregion
    }
}
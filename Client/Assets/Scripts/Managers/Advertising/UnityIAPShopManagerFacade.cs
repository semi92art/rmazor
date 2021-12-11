using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using Utils;

namespace Managers.Advertising
{
    public enum EShopProductResult
    {
        Pending,
        Success,
        Fail
    }
    
    public class ShopItemArgs : EventArgs
    {
        public string                   Price      { get; set; }
        public string                   Currency   { get; set; }
        public bool                     HasReceipt { get; set; }
        public Func<EShopProductResult> Result     { get; set; }
    }
    
    public interface IShopManager : IInit
    {
        void RestorePurchases();
        void Purchase(int _Key, UnityAction _OnPurchase);
        void RateGame();
        ShopItemArgs GetItemInfo(int _Key);
    }
    
    public class UnityIAPShopManagerFacade : UnityIAPShopManager, IShopManager
    {
        #region nonpublic members

        protected override List<ProductInfo> Products => new List<ProductInfo>
        {
            new ProductInfo(PurchaseKeys.Money1, "small_pack_of_coins",           ProductType.Consumable),
            new ProductInfo(PurchaseKeys.Money2, "medium_pack_of_coins",          ProductType.Consumable),
            new ProductInfo(PurchaseKeys.Money3, "big_pack_of_coins",             ProductType.Consumable),
            new ProductInfo(PurchaseKeys.NoAds,  "disable_mandatory_advertising", ProductType.NonConsumable)
        };
        
        private readonly Dictionary<string, UnityAction> m_OnPurchaseActions =
            new Dictionary<string, UnityAction>();

        #endregion

        #region inject

        public UnityIAPShopManagerFacade(ILocalizationManager _LocalizationManager)
            : base(_LocalizationManager) { }

        #endregion


        #region api

        public void Purchase(int _Key, UnityAction _OnPurchase)
        {
#if !UNITY_EDITOR
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                CommonUtils.ShowAlertDialog("OOPS!!!", "No internet connection");
                return;
            }
#endif
            string id = GetProductId(_Key);
            if (m_OnPurchaseActions.ContainsKey(id))
                m_OnPurchaseActions[id] = _OnPurchase;
            else
                m_OnPurchaseActions.Add(id, _OnPurchase);
            BuyProductID(id);
        }

        public void RateGame()
        {
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                string oopsText = LocalizationManager.GetTranslation("oops");
                string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
                CommonUtils.ShowAlertDialog(oopsText, noIntConnText);
                Dbg.LogWarning("Failed to rate game: No internet connection.");
                return;
            }
            
#if UNITY_EDITOR
            Dbg.Log("Rating game available only on device.");
#elif UNITY_ANDROID
            Coroutines.Run(RateGameAndroid());
            // Application.OpenURL ("market://details?id=" + Application.productName);
#elif UNITY_IOS || UNITY_IPHONE
            RateGameIos();
            // Application.OpenURL("itms-apps://itunes.apple.com/us/developer/best-free-games-3d/id959029626");
#endif
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var res = new ShopItemArgs();
            GetProductItemInfo(_Key, ref res);
            return res;
        }

        public override void RestorePurchases()
        {
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                string oopsText = LocalizationManager.GetTranslation("oops");
                string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
                CommonUtils.ShowAlertDialog(oopsText, noIntConnText);
                Dbg.LogWarning("Failed to rate game: No internet connection.");
                return;
            }
            if (!IsInitialized())
            {
                string oopsText = LocalizationManager.GetTranslation("oops");
                string failToRestoreText = LocalizationManager.GetTranslation("service_connection_error");
                CommonUtils.ShowAlertDialog(oopsText, failToRestoreText);
                Dbg.LogWarning("RestorePurchases FAIL. Not initialized.");
                return;
            }
            
            if (Application.platform == RuntimePlatform.IPhonePlayer || 
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Dbg.Log($"{nameof(UnityIAPShopManagerFacade)}: RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions(_Result => {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Dbg.Log($"{nameof(UnityIAPShopManagerFacade)}: " +
                            $"RestorePurchases continuing: " + _Result + ". " +
                            "If no further messages, no purchases available to restore.");
                    if (_Result)
                        SaveUtils.PutValue(SaveKeys.DisableAds, null);
                });
            }
            else
            {
                Dbg.Log($"{nameof(UnityIAPShopManagerFacade)}: " +
                        $"RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
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
                Dbg.Log($"{nameof(UnityIAPShopManagerFacade)}: " +
                        $"ProcessPurchase: PASS. Product: '{_Args.purchasedProduct.definition.id}'");
                kvp.Value?.Invoke();
                m_OnPurchaseActions.Remove(kvp.Key);
                break;
            }
            // Or ... an unknown product has been purchased by this user. Fill in additional products here....
            if (!purchaseActionFound) 
            {
                Dbg.Log($"{nameof(UnityIAPShopManagerFacade)}: ProcessPurchase: FAIL. " +
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
            Dbg.LogError($"{nameof(UnityIAPShopManagerFacade)}: " +
                         $"Get Product Id failed. Product with key {_Key} does not exist");
            return string.Empty;
        }

        private void GetProductItemInfo(int _Key, ref ShopItemArgs _Args)
        {
            var shopProductResult = EShopProductResult.Pending;
            _Args.Result = () => shopProductResult;
            if (!IsInitialized())
            {
                Dbg.LogWarning($"{nameof(UnityIAPShopManagerFacade)}: Get Product Item Info Failed. Not initialized.");
                shopProductResult = EShopProductResult.Fail;
                return;
            }
            string id = GetProductId(_Key);
            var product = m_StoreController.products.set.FirstOrDefault(_P => _P.definition.id == id);
            if (product == null)
            {
                Dbg.LogWarning($"{nameof(UnityIAPShopManagerFacade)}: " +
                               $"Get Product Item Info Failed. Product with id {id} does not exist");
                shopProductResult = EShopProductResult.Fail;
                return;
            }
            _Args.Price = product.metadata.localizedPriceString;
            _Args.HasReceipt = product.hasReceipt;
            shopProductResult = EShopProductResult.Success;
        }
        
#if UNITY_ANDROID

        private IEnumerator RateGameAndroid()
        {
            var reviewManager = new Google.Play.Review.ReviewManager();
            var requestFlowOperation = reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
            {
                // Log error. For example, using requestFlowOperation.Error.ToString().
                yield break;
            }
            var playReviewInfo = requestFlowOperation.GetResult();
            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            yield return launchFlowOperation;
            playReviewInfo = null; // Reset the object
            if (launchFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
            {
                Dbg.LogError($"Failed to rate game: {requestFlowOperation.Error}");
                yield break;
            }
        }
        
#elif UNITY_IPHONE || UNITY_IOS

        private void RateGameIos()
        {
            SA.iOS.StoreKit.ISN_SKStoreReviewController.RequestReview();
        }
        
#endif

        #endregion
    }
}
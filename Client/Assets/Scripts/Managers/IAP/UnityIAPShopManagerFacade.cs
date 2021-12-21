using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using Utils;

namespace Managers.IAP
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
        public Func<EShopProductResult> Result     { get; set; }
    }
    
    public class UnityIAPShopManagerFacade : UnityIAPShopManagerBase, IShopManager
    {
        #region nonpublic members

        protected override List<ProductInfo> Products => new List<ProductInfo>
        {
            new ProductInfo(PurchaseKeys.Money1, $"small_pack_of_coins{(Application.platform == RuntimePlatform.IPhonePlayer ? "_2" : string.Empty)}",           ProductType.Consumable),
            new ProductInfo(PurchaseKeys.Money2, $"medium_pack_of_coins{(Application.platform == RuntimePlatform.IPhonePlayer ? "_2" : string.Empty)}",          ProductType.Consumable),
            new ProductInfo(PurchaseKeys.Money3, $"big_pack_of_coins{(Application.platform == RuntimePlatform.IPhonePlayer ? "_2" : string.Empty)}",             ProductType.Consumable),
            new ProductInfo(PurchaseKeys.NoAds,  $"disable_mandatory_advertising{(Application.platform == RuntimePlatform.IPhonePlayer ? "_2" : string.Empty)}", ProductType.NonConsumable)
        };
        
        private readonly Dictionary<int, UnityAction> m_OnPurchaseActions =
            new Dictionary<int, UnityAction>();

        #endregion

        #region inject

        public UnityIAPShopManagerFacade(ILocalizationManager _LocalizationManager)
            : base(_LocalizationManager) { }

        #endregion

        #region api

        public void Purchase(int _Key)
        {
#if !UNITY_EDITOR
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                CommonUtils.ShowAlertDialog("OOPS!!!", "No internet connection");
                return;
            }
#endif
            string id = GetProductId(_Key);
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
#elif UNITY_IOS || UNITY_IPHONE
            RateGameIos();
#endif
        }

        public ShopItemArgs GetItemInfo(int _Key)
        {
            var res = new ShopItemArgs();
            GetProductItemInfo(_Key, ref res);
            return res;
        }

        public void SetPurchaseAction(int _Key, UnityAction _OnPurchase)
        {
            m_OnPurchaseActions.SetSafe(_Key, _OnPurchase);
        }

        public void SetDeferredAction(int _Key, UnityAction _Action)
        {
            // TODO
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
                        "RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public override PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs _Args)
        {
            string id = _Args.purchasedProduct.definition.id;
            var info = Products
                .FirstOrDefault(_P => _P.Id == _Args.purchasedProduct.definition.id);
            if (info == null)
            {
                Dbg.LogError($"Product with id {id} does not have product");
                return PurchaseProcessingResult.Complete;
            }
            var action = m_OnPurchaseActions.GetSafe(info.Key, out bool containsKey);
            if (!containsKey)
            {
                Dbg.LogError($"Product with id {id} does not have puchase action");
                return PurchaseProcessingResult.Complete;
            }
            action?.Invoke();
            Dbg.Log($"ProcessPurchase: PASS. Product: '{_Args.purchasedProduct.definition.id}'");
            return PurchaseProcessingResult.Complete;
        }

        #endregion
        
        #region nonpublic methods

        private void GetProductItemInfo(int _Key, ref ShopItemArgs _Args)
        {
            _Args.Result = () => EShopProductResult.Pending;
            if (!IsInitialized())
            {
                Dbg.LogWarning($"{nameof(UnityIAPShopManagerFacade)}: Get Product Item Info Failed. Not initialized.");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            string id = GetProductId(_Key);
            var product = m_StoreController.products.set.FirstOrDefault(_P => _P.definition.id == id);
            if (product == null)
            {
                Dbg.LogWarning($"{nameof(UnityIAPShopManagerFacade)}: " +
                               $"Get Product Item Info Failed. Product with id {id} does not exist");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            _Args.Price = product.metadata.localizedPriceString;
            _Args.Result = () => EShopProductResult.Success;
        }
        
#if UNITY_ANDROID

        private IEnumerator RateGameAndroid()
        {
            void OpenAppPageInStoreDirectly() => Application.OpenURL ("market://details?id=" + Application.productName);
            
            var reviewManager = new Google.Play.Review.ReviewManager();
            var requestFlowOperation = reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (!requestFlowOperation.IsSuccessful)
            {
                Dbg.LogWarning($"requestFlowOperation.IsSuccessful: {requestFlowOperation.IsSuccessful}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            if (requestFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
            {
                Dbg.LogWarning($"Failed to load rate game panel: {requestFlowOperation.Error}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            var playReviewInfo = requestFlowOperation.GetResult();
            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            if (!launchFlowOperation.IsSuccessful)
            {
                Dbg.LogWarning($"launchFlowOperation.IsSuccessful: {launchFlowOperation.IsSuccessful}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            yield return launchFlowOperation;
            if (launchFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
            {
                Dbg.LogWarning($"Failed to launch rate game panel: {launchFlowOperation.Error}");
                OpenAppPageInStoreDirectly();
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
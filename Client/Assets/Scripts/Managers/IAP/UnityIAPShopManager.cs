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
    
    public class UnityIAPShopManager : ShopManagerBase, IShopManager, IStoreListener
    {
        #region nonpublic members

        protected          IStoreController   m_StoreController;
        protected          IExtensionProvider m_StoreExtensionProvider;
        
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

        protected ILocalizationManager LocalizationManager { get; }

        public UnityIAPShopManager(ILocalizationManager _LocalizationManager)
        {
            LocalizationManager = _LocalizationManager;
        }

        #endregion

        #region api
        
        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public virtual void Init()
        {
            InitializePurchasing();
        }
        
        public void OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
        {
            m_StoreController = _Controller;
            m_StoreExtensionProvider = _Extensions;
            Dbg.Log($"{nameof(OnInitialized)}");
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void OnInitializeFailed(InitializationFailureReason _Error)
        {
            Dbg.LogWarning($"{nameof(OnInitializeFailed)}" +
                           $" {nameof(InitializationFailureReason)}:" + _Error);
        }
        
        public void OnPurchaseFailed(Product _Product, PurchaseFailureReason _FailureReason)
        {
            Dbg.LogWarning($"{nameof(OnPurchaseFailed)}" +
                           $" {nameof(PurchaseFailureReason)}:" + _FailureReason);
        }

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

        public void RestorePurchases()
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
                Dbg.Log($"{nameof(UnityIAPShopManager)}: RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions(_Result => {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    Dbg.Log($"{nameof(UnityIAPShopManager)}: " +
                            $"RestorePurchases continuing: " + _Result + ". " +
                            "If no further messages, no purchases available to restore.");
                    if (_Result)
                        SaveUtils.PutValue(SaveKeys.DisableAds, null);
                });
            }
            else
            {
                Dbg.Log($"{nameof(UnityIAPShopManager)}: " +
                        "RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs _Args)
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
        
        protected void InitializePurchasing() 
        {
            // // If we have already connected to Purchasing ...
            // if (IsInitialized())
            //     return;
            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            // Add a product to sell / restore by way of its identifier, associating the general identifier
            // with its store-specific identifiers.
            foreach (var kvp in Products)
                builder.AddProduct(kvp.Id, kvp.Type);
            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }
        
        protected bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }
        
        protected void BuyProductID(string _ProductId)
        {
            if (!IsInitialized())
            {
                string oopsText = LocalizationManager.GetTranslation("oops");
                string failToBuyProduct = LocalizationManager.GetTranslation("service_connection_error");
                CommonUtils.ShowAlertDialog(oopsText, failToBuyProduct);
                Dbg.LogWarning("BuyProductID FAIL. Not initialized.");
                return;
            }
            var product = m_StoreController.products.WithID(_ProductId);
            if (product == null || !product.availableToPurchase)
            {
                Dbg.LogWarning("BuyProductID: FAIL. Not purchasing product, " +
                               "either is not found or is not available for purchase");
                return;
            }
            Dbg.Log($"Purchasing product asychronously: '{product.definition.id}'");
            m_StoreController.InitiatePurchase(product);
        }

        private void GetProductItemInfo(int _Key, ref ShopItemArgs _Args)
        {
            _Args.Result = () => EShopProductResult.Pending;
            if (!IsInitialized())
            {
                Dbg.LogWarning($"{nameof(UnityIAPShopManager)}: Get Product Item Info Failed. Not initialized.");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            string id = GetProductId(_Key);
            var product = m_StoreController.products.set.FirstOrDefault(_P => _P.definition.id == id);
            if (product == null)
            {
                Dbg.LogWarning($"{nameof(UnityIAPShopManager)}: " +
                               $"Get Product Item Info Failed. Product with id {id} does not exist");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            _Args.Price = product.metadata.localizedPriceString;
            _Args.Result = () => EShopProductResult.Success;
        }
        
#if UNITY_ANDROID

        private System.Collections.IEnumerator RateGameAndroid()
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
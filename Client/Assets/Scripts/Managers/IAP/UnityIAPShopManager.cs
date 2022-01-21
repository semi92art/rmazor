using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Utils;
using RMAZOR;
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
        public bool                     HasReceipt { get; set; }
        public Func<EShopProductResult> Result     { get; set; }
    }
    
    public class UnityIAPShopManager : ShopManagerBase, IStoreListener
    {
        #region nonpublic members

        protected IStoreController   m_StoreController;
        protected IExtensionProvider m_StoreExtensionProvider;

        protected readonly Dictionary<int, UnityAction> m_OnPurchaseActions =
            new Dictionary<int, UnityAction>();
        protected readonly Dictionary<int, UnityAction> m_OnDeferredActions =
            new Dictionary<int, UnityAction>();

        #endregion

        #region inject

        private ILocalizationManager LocalizationManager { get; }

        public UnityIAPShopManager(ILocalizationManager _LocalizationManager)
        {
            LocalizationManager = _LocalizationManager;
        }

        #endregion

        #region api

        public override void Init()
        {
            InitializePurchasing();
        }
        
        public virtual void OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
        {
            m_StoreController = _Controller;
            m_StoreExtensionProvider = _Extensions;
            Dbg.Log($"{nameof(UnityIAPShopManager)} {nameof(OnInitialized)}");
            base.Init();
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

        public override void Purchase(int _Key)
        {
#if !UNITY_EDITOR
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                CommonUtils.ShowAlertDialog("OOPS!!!", "No internet connection");
                return;
            }
#endif
            Dbg.Log(nameof(Purchase) + ": " + _Key);
            string id = GetProductId(_Key);
            BuyProductID(id);
        }

        public override void RateGame(bool _JustSuggest = true)
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
            Cor.Run(RateGameAndroid(_JustSuggest));
#elif UNITY_IOS || UNITY_IPHONE
            RateGameIos();
#endif
        }

        public override ShopItemArgs GetItemInfo(int _Key)
        {
            var res = new ShopItemArgs();
            GetProductItemInfo(_Key, ref res);
            return res;
        }

        public override void SetPurchaseAction(int _Key, UnityAction _OnPurchase)
        {
            m_OnPurchaseActions.SetSafe(_Key, _OnPurchase);
        }

        public override void SetDeferredAction(int _Key, UnityAction _Action)
        {
            m_OnDeferredActions.SetSafe(_Key, _Action);
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
            foreach (var product in Products.Where(_P => _P.Type == ProductType.NonConsumable))
            {
                var info = GetItemInfo(product.Key);
                Cor.Run(Cor.WaitWhile(
                    () => info.Result() == EShopProductResult.Pending,
                    () =>
                    {
                        if (info.Result() == EShopProductResult.Fail)
                            return;
                        if (info.HasReceipt)
                            m_OnPurchaseActions[product.Key]?.Invoke();
                    }));
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
                Dbg.LogWarning($"Product with id {id} does not have purchase action");
                return PurchaseProcessingResult.Complete;
            }
            action?.Invoke();
            Dbg.Log($"ProcessPurchase: PASS. Product: '{_Args.purchasedProduct.definition.id}'");
            return PurchaseProcessingResult.Complete;
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitializePurchasing()
        {
            var builder = GetBuilder();
            foreach (var kvp in Products)
                builder.AddProduct(kvp.Id, kvp.Type);
            UnityPurchasing.Initialize(this, builder);
        }

        protected virtual ConfigurationBuilder GetBuilder()
        {
            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);
            return builder;
        }
        
        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }
        
        private void BuyProductID(string _ProductId)
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
            _Args.HasReceipt = product.hasReceipt;
            _Args.Currency = product.metadata.isoCurrencyCode;
            _Args.Price = product.metadata.localizedPriceString;
            _Args.Result = () => EShopProductResult.Success;
        }
        
        protected void OnDeferredPurchase(Product _Product)
        {
            var key = Products.FirstOrDefault(_P => _P.Id == _Product.definition.id)?.Key;
            if (!key.HasValue)
            {
                Dbg.LogError($"Purchase of {_Product.definition.id} was not deferred");
                return;
            }
            m_OnDeferredActions[key.Value]?.Invoke();
            Dbg.Log($"Purchase of {_Product.definition.id} is deferred");
        }
        
#if UNITY_ANDROID

        private IEnumerator RateGameAndroid(bool _JustSuggest)
        {
            if (_JustSuggest)
            {
                string title = LocalizationManager.GetTranslation("rate_dialog_title");
                string text = LocalizationManager.GetTranslation("rate_dialog_text") + "\n" +
                    "\u2B50\u2B50\u2B50\u2B50\u2B50";
                string ok = LocalizationManager.GetTranslation("rate_yes");
                string notNow = LocalizationManager.GetTranslation("rate_not_now");
                string never = LocalizationManager.GetTranslation("rate_never");
                MTAssets.NativeAndroidToolkit.NativeAndroid.Dialogs.ShowNeutralDialog(title, text, ok, notNow, never);
                MTAssets.NativeAndroidToolkit.Events.DialogsEvents.onNeutralYes = () =>
                {
                    Cor.Run(RateGameAndroid(false));
                };
                MTAssets.NativeAndroidToolkit.Events.DialogsEvents.onNeutralNo = () =>
                {

                };
                MTAssets.NativeAndroidToolkit.Events.DialogsEvents.onNeutralNeutral = () =>
                {
                    SaveUtils.PutValue(SaveKeys.GameWasRated, true);
                };
                yield break;
            }
            
            static void OpenAppPageInStoreDirectly()
            {
                Application.OpenURL("market://details?id=" + Application.productName);
            }

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
            SaveUtils.PutValue(SaveKeys.GameWasRated, true);
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
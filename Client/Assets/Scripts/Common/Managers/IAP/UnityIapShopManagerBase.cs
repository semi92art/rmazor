using System;
using System.Collections.Generic;
using System.Linq;
using Common.Utils;
using UnityEngine.Events;
using UnityEngine.Purchasing;

namespace Common.Managers.IAP
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
    
    public abstract class UnityIapShopManagerBase : ShopManagerBase, IStoreListener
    {
        #region nonpublic members

        private IStoreController   m_StoreController;
        private IExtensionProvider m_StoreExtensionProvider;

        private readonly Dictionary<int, Dictionary<int,UnityAction>> m_OnPurchaseActionsDictDict =
            new Dictionary<int, Dictionary<int,UnityAction>>();
        private readonly Dictionary<int, Dictionary<int,UnityAction>> m_OnDeferredActionsDictDict =
            new Dictionary<int, Dictionary<int,UnityAction>>();

        #endregion

        #region inject

        private ILocalizationManager LocalizationManager { get; }

        protected UnityIapShopManagerBase(ILocalizationManager _LocalizationManager)
        {
            LocalizationManager = _LocalizationManager;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            InitializePurchasing();
        }
        
        public virtual void OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
        {
            m_StoreController = _Controller;
            m_StoreExtensionProvider = _Extensions;
            Dbg.Log($"{nameof(UnityIapShopManagerBase)} {nameof(OnInitialized)}");
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
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                string oopsText = LocalizationManager.GetTranslation("oops");
                string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
                CommonUtils.ShowAlertDialog(oopsText, noIntConnText);
                return;
            }
            Dbg.Log(nameof(Purchase) + ": " + _Key);
            string id = GetProductId(_Key);
            BuyProductID(id);
        }

        public override bool RateGame()
        {
            if (NetworkUtils.IsInternetConnectionAvailable())
                return true;
            string oopsText = LocalizationManager.GetTranslation("oops");
            string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
            CommonUtils.ShowAlertDialog(oopsText, noIntConnText);
            Dbg.LogWarning("Failed to rate game: No internet connection.");
            return false;
        }

        public override ShopItemArgs GetItemInfo(int _Key)
        {
            var res = new ShopItemArgs();
            GetProductItemInfo(_Key, ref res);
            return res;
        }

        public override void AddPurchaseAction(int _ProductKey, int _ActionKey, UnityAction _Action)
        {
            AddTranstactionAction(_ProductKey, _ActionKey, _Action, false);
        }

        public override void AddDeferredAction(int _ProductKey, int _ActionKey, UnityAction _Action)
        {
            AddTranstactionAction(_ProductKey, _ActionKey, _Action, true);
        }

        public override void RestorePurchases()
        {
            if (!NetworkUtils.IsInternetConnectionAvailable())
            {
                string oopsText = LocalizationManager.GetTranslation("oops");
                string noIntConnText = LocalizationManager.GetTranslation("no_internet_connection");
                CommonUtils.ShowAlertDialog(oopsText, noIntConnText);
                Dbg.LogWarning("Failed to restore purchases: No internet connection.");
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
                        {
                            if (m_OnPurchaseActionsDictDict.ContainsKey(product.Key))
                            {
                                var actionsDict = m_OnPurchaseActionsDictDict[product.Key];
                                foreach (var action in actionsDict.Values)
                                {
                                    action?.Invoke();
                                }
                            }
                        }
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
            if (!m_OnPurchaseActionsDictDict.ContainsKey(info.Key))
            {
                Dbg.LogWarning($"Product with id {id} does not have purchase action");
                return PurchaseProcessingResult.Complete;
            }
            var actions = m_OnPurchaseActionsDictDict[info.Key];
            foreach (var action in actions.Values)
            {
                action?.Invoke();
            }
            Dbg.Log($"ProcessPurchase: PASS. Product: '{_Args.purchasedProduct.definition.id}'");
            return PurchaseProcessingResult.Complete;
        }

        #endregion
        
        #region nonpublic methods
        
        private void AddTranstactionAction(int _ProductKey, int _ActionKey, UnityAction _Action, bool _Deferred)
        {
            var actionsDictDict = _Deferred ?
                m_OnDeferredActionsDictDict : m_OnPurchaseActionsDictDict;
            if (!actionsDictDict.ContainsKey(_ProductKey))
                actionsDictDict.Add(_ProductKey, new Dictionary<int, UnityAction>());
            var actionsDict = actionsDictDict[_ProductKey];
            if (!actionsDict.ContainsKey(_ActionKey))
                actionsDict.Add(_ActionKey, _Action);
        }
        
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
            Dbg.Log($"Purchasing product asynchronously: '{product.definition.id}'");
            m_StoreController.InitiatePurchase(product);
        }

        private void GetProductItemInfo(int _Key, ref ShopItemArgs _Args)
        {
            _Args.Result = () => EShopProductResult.Pending;
            if (!IsInitialized())
            {
                Dbg.LogWarning($"{nameof(UnityIapShopManagerBase)}: Get Product Item Info Failed. Not initialized.");
                _Args.Result = () => EShopProductResult.Fail;
                return;
            }
            string id = GetProductId(_Key);
            var product = m_StoreController.products.set.FirstOrDefault(_P => _P.definition.id == id);
            if (product == null)
            {
                Dbg.LogWarning($"{nameof(UnityIapShopManagerBase)}: " +
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
            string id = _Product.definition.id;
            var info = Products.FirstOrDefault(_P => _P.Id == _Product.definition.id);
            if (info == null)
            {
                Dbg.LogError($"Product with id {id} does not have product");
                return;
            }
            if (!m_OnPurchaseActionsDictDict.ContainsKey(info.Key))
            {
                Dbg.LogWarning($"Product with id {id} does not have purchase action");
                return;
            }
            var actions = m_OnPurchaseActionsDictDict[info.Key];
            foreach (var action in actions.Values)
            {
                action?.Invoke();
                if (action != null)
                {
                    Dbg.Log($"Purchase of {_Product.definition.id} is deferred");
                    action();

                }
                else
                {
                    Dbg.LogWarning("Deferred action was not set of " +
                                   $"product with id {_Product.definition.id} was not set" +
                                   ", but it's okay, let this product be free");
                }
            }
        }

        #endregion
    }
}
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using Utils;

namespace Managers.IAP
{
    public abstract class UnityIAPShopManagerBase : ShopManagerBase, IStoreListener, IInit
    {

        
        #region nonpublic members

        protected abstract override List<ProductInfo> Products { get; }
        protected          IStoreController   m_StoreController;        // The Unity Purchasing system.
        protected          IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        
        #endregion

        #region inject
        
        protected ILocalizationManager LocalizationManager { get; }

        protected UnityIAPShopManagerBase(ILocalizationManager _LocalizationManager)
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

        public abstract PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs _Args);
        public abstract void RestorePurchases();

        public void OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
        {
            m_StoreController = _Controller;
            m_StoreExtensionProvider = _Extensions;
            Dbg.Log($"{nameof(UnityIAPShopManagerBase)} {nameof(OnInitialized)}");
            Initialize?.Invoke();
            Initialized = true;
        }

        public void OnInitializeFailed(InitializationFailureReason _Error)
        {
            Dbg.LogWarning($"{nameof(UnityIAPShopManagerBase)} {nameof(OnInitializeFailed)}" +
                           $" {nameof(InitializationFailureReason)}:" + _Error);
        }
        
        public void OnPurchaseFailed(Product _Product, PurchaseFailureReason _FailureReason)
        {
            Dbg.LogWarning($"{nameof(UnityIAPShopManagerBase)} {nameof(OnPurchaseFailed)}" +
                           $" {nameof(PurchaseFailureReason)}:" + _FailureReason);
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

        #endregion
    }
}
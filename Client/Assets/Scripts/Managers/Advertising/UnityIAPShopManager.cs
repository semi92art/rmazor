using System.Collections.Generic;
using UnityEngine.Purchasing;
using Utils;

namespace Managers.Advertising
{
    public abstract class UnityIAPShopManager : IStoreListener
    {
        #region types

        protected class ProductInfo
        {
            public int Key { get; }
            public string Id { get; }
            public ProductType Type { get; }
            
            public ProductInfo(int _Key, string _Id, ProductType _Type)
            {
                Key = _Key;
                Id = _Id;
                Type = _Type;
            }
        }

        #endregion
        
        #region nonpublic members
     
        protected abstract List<ProductInfo> Products { get; }
        protected IStoreController m_StoreController;          // The Unity Purchasing system.
        protected IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        
        #endregion
        
        #region api

        public abstract PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs _Args);
        public abstract void RestorePurchases();

        public void OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
        {
            m_StoreController = _Controller;         // Overall Purchasing system, configured with products for this application.
            m_StoreExtensionProvider = _Extensions;  // Store specific subsystem, for accessing device-specific store features.
        }

        public void OnInitializeFailed(InitializationFailureReason _Error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            Dbg.LogWarning("OnInitializeFailed InitializationFailureReason:" + _Error);
        }
        
        public void OnPurchaseFailed(Product _Product, PurchaseFailureReason _FailureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            Dbg.LogWarning("OnPurchaseFailed: FAIL. Product: " +
                           $"'{_Product.definition.storeSpecificId}', PurchaseFailureReason: {_FailureReason}");
        }
        
        #endregion
        
        #region nonpublic methods
        
        protected void InitializePurchasing() 
        {
            // If we have already connected to Purchasing ...
            if (IsInitialized())
                return;
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
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }
        
        protected void BuyProductID(string _ProductId)
        {
            if (!IsInitialized())
            {
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
            // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
            // asynchronously.
            m_StoreController.InitiatePurchase(product);
        }

        #endregion
    }
}
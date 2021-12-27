using System.Collections.Generic;
using System.Linq;
using UnityEngine.Purchasing;
using Utils;

namespace Managers.IAP
{
    public abstract class ShopManagerBase
    {
        #region types

        protected class ProductInfo
        {
            public int         Key  { get; }
            public string      Id   { get; }
            public ProductType Type { get; }
            
            public ProductInfo(int _Key, string _Id, ProductType _Type)
            {
                Key = _Key;
                Id = _Id;
                Type = _Type;
            }
        }

        #endregion

        protected abstract List<ProductInfo> Products { get; }
        
        protected string GetProductId(int _Key)
        {
            var product = Products.FirstOrDefault(_P => _P.Key == _Key);
            if (product != null) 
                return product.Id;
            Dbg.LogError($"{nameof(UnityIAPShopManager)}: " +
                         $"Get Product Id failed. Product with key {_Key} does not exist");
            return string.Empty;
        }
    }
}
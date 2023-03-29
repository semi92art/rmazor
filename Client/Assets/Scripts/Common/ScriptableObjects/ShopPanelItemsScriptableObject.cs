using System;
using Common.Helpers;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(fileName = "shop_items_set", menuName = "Configs and Sets/Shop Money Items Set")]
    public class ShopPanelItemsScriptableObject : ScriptableObject
    {
        [Serializable]
        public class ShopMoneyAssetItem
        {
            public Sprite icon;
            public int    purchaseKey;
            public int    reward;
            public bool   watchingAds;
        }
        
        [Serializable]
        public class ShopMoneyAssetItemSet : ReorderableArray<ShopMoneyAssetItem> { }

        [Header("Set"), Reorderable(paginate = true, pageSize = 10)]
        public ShopMoneyAssetItemSet set;
    }
}
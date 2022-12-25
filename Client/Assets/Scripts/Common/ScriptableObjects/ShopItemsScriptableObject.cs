using System;
using Common.Helpers;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(fileName = "shop_items_set", menuName = "Configs and Sets/Shop Items Set")]
    public class ShopItemsScriptableObject : ScriptableObject
    {
        [Serializable]
        public class ShopItem
        {
            public int    purchaseId;
            public Sprite icon;
            public int    price;
            public bool   watchingAds;
        }
        
        [Serializable]
        public class ShopItemSet : ReorderableArray<ShopItem> { }

        [Header("Set"), Reorderable(paginate = true, pageSize = 10)]
        public ShopItemSet set;
    }
}
using System;
using Malee.List;
using UnityEngine;

namespace ScriptableObjects
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
            public int    unlockingLevel = -1;
        }
        
        [Serializable]
        public class ShopItemSet : ReorderableArray<ShopItem> { }

        [Header("Set"), Reorderable(paginate = true, pageSize = 10)]
        public ShopItemSet set;
    }
}
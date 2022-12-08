using System;
using System.Linq;
using Common.Helpers.Attributes;
using UnityEngine;

namespace Common.Helpers
{
    [DisallowMultipleComponent]
    public class PrefabContent : MonoBehaviour
    {
        [Serializable]
        public class ContentItem
        {
            public string     name;
            public GameObject item;
        }
    
        [Serializable]
        public class ContentList : ReorderableArray<ContentItem>
        { }
    
        [Header("Content"), Reorderable(paginate = true, pageSize = 20)]
        public ContentList content;

        public GameObject GetItem(string _Name)
        {
            return content.FirstOrDefault(_Ci => _Ci.name == _Name)?.item;
        }

        public T GetItemComponent<T>(string _Name) where T : Component
        {
            return GetItem(_Name).GetComponent<T>();
        }

        private void Start()
        {
            var missingItems = content.Where(_Item => _Item.item == null).ToArray();
            if (!missingItems.Any())
                return;
            foreach (var missingItem in missingItems)
                Dbg.LogWarning($"Missing item {missingItem.name} from prefab {name}");
        }
    }
}
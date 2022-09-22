using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Common.Entities
{
    [CreateAssetMenu(fileName = "new_set", menuName = "Configs and Sets/Prefab Set", order = 1)]
    public class PrefabSetScriptableObject : ScriptableObject
    {
        [Serializable]
        public class Prefab
        {
            public Object item;
            public string name;
            public bool   bundle;
        }
    
        [Serializable]
        public class PrefabsList : ReorderableArray<Prefab> { }

        [Header("Prefabs"), Reorderable(paginate = true, pageSize = 50)]
        public PrefabsList prefabs;
    }
}



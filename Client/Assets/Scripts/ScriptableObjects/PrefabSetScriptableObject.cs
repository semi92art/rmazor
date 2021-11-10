using System;
using Malee.List;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "new_set", menuName = "Configs and Sets/Prefab Set", order = 1)]
    public class PrefabSetScriptableObject : ScriptableObject
    {
        [Serializable]
        public class Prefab
        {
            public Object item;
            public string name;
        }
    
        [Serializable]
        public class PrefabsList : ReorderableArray<Prefab> { }

        public bool bundles;
        [Header("Prefabs"), Reorderable(paginate = true, pageSize = 50)]
        public PrefabsList prefabs;
    }
}



using System;
using Malee.List;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Entities
{
    [Serializable]
    public class Prefab
    {
        public Object item;
        public string name;
    }
    
    [Serializable]
    public class PrefabsList : ReorderableArray<Prefab> { }

    [CreateAssetMenu(fileName = "new_set", menuName = "Prefab Set", order = 1)]
    public class PrefabSetObject : ScriptableObject
    {
        #region public fields

        public bool bundles;
        [Header("Prefabs"), Reorderable(paginate = true, pageSize = 20)]
        public PrefabsList prefabs;

        #endregion
    }
}



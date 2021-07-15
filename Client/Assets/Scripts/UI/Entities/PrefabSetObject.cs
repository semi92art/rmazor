﻿using Malee.List;
using UnityEngine;

namespace UI.Entities
{
    [System.Serializable]
    public class Prefab
    {
        public Object item;
        public string name;
    }
    
    [System.Serializable]
    public class PrefabsList : ReorderableArray<Prefab> { }

    [CreateAssetMenu(fileName = "new_set", menuName = "Prefab Set", order = 1)]
    public class PrefabSetObject : ScriptableObject
    {
        #region public fields
        
        [Header("Prefabs"), Reorderable(paginate = true, pageSize = 20)]
        public PrefabsList prefabs;

        #endregion
    }
}



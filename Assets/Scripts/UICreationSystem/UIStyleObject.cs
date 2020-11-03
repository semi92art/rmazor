using UnityEngine;
using UnityEngine.UI;
using Malee.List;

namespace UICreationSystem
{
    [System.Serializable]
    public class Prefab
    {
        public Object item;
        public string name;
    }
    
    [System.Serializable]
    public class PrefabsList : ReorderableArray<Prefab>
    { }
    
    [CreateAssetMenu(fileName = "new_style", menuName = "Style", order = 1)]
    public class UIStyleObject : ScriptableObject
    {
        #region public fields
        
        [Header("Prefabs"), Reorderable(paginate = true, pageSize = 20)]
        public PrefabsList prefabs;

        #endregion
    }
}



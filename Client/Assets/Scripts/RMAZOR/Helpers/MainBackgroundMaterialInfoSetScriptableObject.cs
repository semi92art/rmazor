using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;

namespace RMAZOR.Helpers
{
    [Serializable]
    public class MainBackgroundMaterialInfo
    {
        public string   name;
        public Material material;
    }
    
    [Serializable]
    public class MainBackgroundMaterialInfoSet : ReorderableArray<MainBackgroundMaterialInfo> { }
    
    [CreateAssetMenu(
        fileName = "main_background_set", 
        menuName = "Configs and Sets/Main Background Set",
        order = 0)]
    public class MainBackgroundMaterialInfoSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public MainBackgroundMaterialInfoSet set;
    }
}
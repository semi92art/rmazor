using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;

namespace RMAZOR.Helpers
{
    [Serializable]
    public class AdditionalBackgroundInfo
    {
        public string name;
        public Sprite sprite;
    }
    
    [Serializable]
    public class AdditionalBackgroundInfoSet : ReorderableArray<AdditionalBackgroundInfo> { }
    
    
    [CreateAssetMenu(
        fileName = "additional_background_set",
        menuName = "Configs and Sets/Additional Background Set",
        order = 0)]
    public class AdditionalBackgroundInfoSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public AdditionalBackgroundInfoSet set;
    }
}
using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace Common.Entities
{
    [Serializable]
    public class MainColorsSetItem
    {
        [JsonProperty(PropertyName = "N")] public string name;
        [JsonProperty(PropertyName = "C")] public Color  color;
    }
        
    [Serializable]
    public class MainColorsItemSet : ReorderableArray<MainColorsSetItem> { }
    
    [CreateAssetMenu(fileName = "color_set", menuName = "Configs and Sets/Color Set", order = 0)]
    public class MainColorsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public MainColorsItemSet set;
    }
}
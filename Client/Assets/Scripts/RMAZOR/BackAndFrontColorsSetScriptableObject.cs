using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    public class BackAndFrontColorsSetItem
    {
        [JsonProperty(PropertyName = "M")] public Color main;
        [JsonProperty(PropertyName = "B1")] public Color bacground1;
        [JsonProperty(PropertyName = "B2")] public Color bacground2;
    }
    
    [Serializable]
    public class BackAndFrontColorsSet : ReorderableArray<BackAndFrontColorsSetItem> { }
    
    [CreateAssetMenu(fileName = "back_and_front_colors_set", menuName = "Configs and Sets/Back And Front Color Set", order = 0)]
    public class BackAndFrontColorsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public BackAndFrontColorsSet set;
    }
}
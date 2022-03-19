using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    public class BackAndFrontColorsSetItem
    {
        public Color main;
        public Color bacground1;
        public Color bacground2;
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
using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;

namespace Common.Entities
{
    [Serializable]
    public class MainColorsProps
    {
        public string name;
        public Color  color;
    }
        
    [Serializable]
    public class MainColorsPropsSet : ReorderableArray<MainColorsProps> { }
    
    [CreateAssetMenu(fileName = "color_set", menuName = "Configs and Sets/Color Set", order = 0)]
    public class MainColorsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public MainColorsPropsSet set;
    }
}
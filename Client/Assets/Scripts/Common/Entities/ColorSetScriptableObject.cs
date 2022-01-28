using System;
using Malee.List;
using UnityEngine;

namespace Common.Entities
{
    [Serializable]
    public class ColorSetItem
    {
        public string name;
        public Color  color;
    }
        
    [Serializable]
    public class ColorItemSet : ReorderableArray<ColorSetItem> { }
    
    [CreateAssetMenu(fileName = "color_set", menuName = "Configs and Sets/Color Set", order = 0)]
    public class ColorSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public ColorItemSet set;
    }
}
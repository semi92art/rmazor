using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class CirclesTextureSetItem
    {
        [Range(0, 0.2f)] public float                           radius;
        [Range(0, 20)]   public int                             wavesCount;
        [Range(0, 1)]    public float                           amplitude;
        public                  EBackgroundCircleCenterPosition center;
    }
        
    [Serializable]
    public class CirclesTextureSet : ReorderableArray<CirclesTextureSetItem> { }
    
    [CreateAssetMenu(fileName = "circles_texture_set", menuName = "Configs and Sets/Circles Texture Set", order = 0)]
    public class CirclesTexturePropertiesSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public CirclesTextureSet set;
    }
}
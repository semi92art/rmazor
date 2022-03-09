using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class TrianglesTextureSetItem : LinesTextureSetItem
    {
        [Range(1, 10)] public float size;
        [Range(1, 2)] public float ratio;
        public float a;
        public float b;
    }
    
    [Serializable]
    public class TrianglesTextureSet : ReorderableArray<TrianglesTextureSetItem> { }
    
    [CreateAssetMenu(fileName = "triangles_texture_set", menuName = "Configs and Sets/Triangles Texture Set")]
    public class TrianglesTexturePropertiesSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public TrianglesTextureSet set;
    }
}
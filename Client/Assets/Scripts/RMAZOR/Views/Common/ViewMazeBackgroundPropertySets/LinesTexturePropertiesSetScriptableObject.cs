using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class LinesTextureSetItem
    {
        [Range(1, 500)] public int   tiling;
        [Range(0, 1)]   public float direction;
        [Range(0, 1)]   public float wrapScale;
        [Range(1, 10)]  public float wrapTiling;
    }
        
    [Serializable]
    public class LinesTextureSet : ReorderableArray<LinesTextureSetItem> { }
    
    [CreateAssetMenu(fileName = "lines_texture_set", menuName = "Configs and Sets/Lines Texture Set", order = 0)]
    public class LinesTexturePropertiesSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public LinesTextureSet set;
    }
}
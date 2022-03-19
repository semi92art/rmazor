using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class LinesTextureSetItem
    {
        [Range(1, 10), JsonProperty(PropertyName = "L1")]
        public int tiling;
        [Range(0, 1), JsonProperty(PropertyName = "L2")]
        public float direction;
        [Range(-1, 2), JsonProperty(PropertyName = "L3")]
        public float wrapScale;
        [Range(1, 10), JsonProperty(PropertyName = "L4")]
        public float wrapTiling;
    }

    [Serializable]
    public class LinesTextureSet : ReorderableArray<LinesTextureSetItem> { }

    [CreateAssetMenu(fileName = "lines_texture_set", menuName = "Configs and Sets/Lines Texture Set")]
    public class LinesTexturePropertiesSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public LinesTextureSet set;
    }
}
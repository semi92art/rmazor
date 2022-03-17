using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class Circles2TextureSetItem : LinesTextureSetItem
    {
        [Range(0.05f, 0.5f), JsonProperty(PropertyName = "C2_1")]
        public float radius;
        [Range(0.05f, 0.5f), JsonProperty(PropertyName = "C2_2")]
        public float stepX;
        [Range(0.05f, 0.5f), JsonProperty(PropertyName = "C2_3")]
        public float stepY;
        [JsonProperty(PropertyName = "C2_4")] public bool alternateX;
    }

    [Serializable]
    public class Circles2TextureSet : ReorderableArray<Circles2TextureSetItem> { }

    [CreateAssetMenu(fileName = "circles2_texture_set", menuName = "Configs and Sets/Circles 2 Texture Set")]
    public class Circles2TexturePropertiesSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public Circles2TextureSet set;
    }
}
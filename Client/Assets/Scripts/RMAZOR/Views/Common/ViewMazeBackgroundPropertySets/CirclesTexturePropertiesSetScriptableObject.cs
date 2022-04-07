using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class CirclesTextureSetItem : LinesTextureSetItem
    {
        [Range(0, 0.2f), JsonProperty(PropertyName = "C1")]
        public float radius;
        [Range(0, 20), JsonProperty(PropertyName = "C2")]
        public int wavesCount;
        [Range(0, 1), JsonProperty(PropertyName = "C3")]
        public float amplitude;
        [JsonProperty(PropertyName = "C4")] public EBackgroundCircleCenterPosition center;
        
        public override string ToString(string _Format, IFormatProvider _FormatProvider)
        {
            return nameof(radius) + " " + radius + ", "
                   + nameof(wavesCount) + " " + wavesCount + ", "
                   + nameof(amplitude) + " " + amplitude + ", "
                   + nameof(center) + " " + center + ", "
                   + base.ToString(_Format, _FormatProvider);
        }
    }

    [Serializable]
    public class CirclesTextureSet : ReorderableArray<CirclesTextureSetItem> { }

    [CreateAssetMenu(fileName = "circles_texture_set", menuName = "Configs and Sets/Circles Texture Set")]
    public class CirclesTexturePropertiesSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public CirclesTextureSet set;
    }
}
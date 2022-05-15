using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    public interface ITextureProps : IFormattable
    {
        bool InUse { get; }
    }
    
    [Serializable]
    public class TexturePropsBase : ITextureProps
    {
        [JsonProperty(PropertyName = "U")]
        public bool inUse;
        [Range(1, 10), JsonProperty(PropertyName = "L1")]
        public int tiling;
        [Range(0, 1), JsonProperty(PropertyName = "L2")]
        public float direction;
        [Range(-1, 2), JsonProperty(PropertyName = "L3")]
        public float wrapScale;
        [Range(1, 10), JsonProperty(PropertyName = "L4")]
        public float wrapTiling;

        public virtual string ToString(string _Format, IFormatProvider _FormatProvider)
        {
            return nameof(tiling) + " " + tiling + ", "
                   + nameof(direction) + " " + direction + ", "
                   + nameof(wrapScale) + " " + wrapScale + ", "
                   + nameof(wrapScale) + " " + wrapScale + ". ";
        }

        [JsonIgnore] public bool InUse => inUse;
    }
}
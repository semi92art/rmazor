using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class Triangles2TextureProps : ITextureProps
    {
        [JsonProperty(PropertyName = "U")]   public bool  inUse;
        [JsonProperty(PropertyName = "T1")]  public float size;
        [JsonProperty(PropertyName = "T2")]  public float ratio;
        [JsonProperty(PropertyName = "T3")]  public float a;
        [JsonProperty(PropertyName = "T4")]  public float b;
        [JsonProperty(PropertyName = "T5")]  public float c;
        [JsonProperty(PropertyName = "T6")]  public float d;
        [JsonProperty(PropertyName = "T7")]  public float e;
        [JsonProperty(PropertyName = "T8")]  public int   f;
        [JsonProperty(PropertyName = "T9")]  public bool  smooth;
        [JsonProperty(PropertyName = "T10")] public bool  mirror;
        [JsonProperty(PropertyName = "T11")] public bool  trunc;
        [JsonProperty(PropertyName = "T12")] public bool  truncColor2;
        [JsonProperty(PropertyName = "T13")] public float direction;

        [JsonIgnore] public bool   InUse => inUse;
        public string ToString(string _Format, IFormatProvider _FormatProvider)
        {
            return nameof(inUse) + " " + inUse + ", "
                   + nameof(size) + " " + size + ", "
                   + nameof(a) + " " + a + ", "
                   + nameof(b) + " " + b + ", "
                   + nameof(c) + " " + c + ", "
                   + nameof(d) + " " + d + ", "
                   + nameof(e) + " " + e + ", "
                   + nameof(f) + " " + f + ", "
                   + nameof(smooth) + " " + smooth + ", "
                   + nameof(mirror) + " " + mirror + ", "
                   + nameof(trunc) + " " + trunc + ", "
                   + nameof(truncColor2) + " " + truncColor2 + ", "
                   + nameof(direction) + " " + direction;
        }
    }

    [Serializable]
    public class Triangles2TexturePropsSet : ReorderableArray<Triangles2TextureProps> { }

    [CreateAssetMenu(fileName = "triangles_texture_2_set", menuName = "Configs and Sets/Triangles Texture 2 Set")]
    public class Triangles2TexturePropsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public Triangles2TexturePropsSet set;
    }
}
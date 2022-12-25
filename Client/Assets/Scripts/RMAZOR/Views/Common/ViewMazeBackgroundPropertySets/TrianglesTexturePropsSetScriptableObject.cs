using System;
using Common.Helpers;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewMazeBackgroundPropertySets
{
    [Serializable]
    public class TrianglesTextureProps : TexturePropsBase
    {
        [Range(1, 10), JsonProperty(PropertyName = "T1")]
        public float size;
        [Range(1, 2), JsonProperty(PropertyName = "T2")]
        public float ratio;
        [JsonProperty(PropertyName = "T3")] public float a;
        [JsonProperty(PropertyName = "T4")] public float b;
        
        public override string ToString(string _Format, IFormatProvider _FormatProvider)
        {
            return nameof(size) + " " + size + ", "
                   + nameof(ratio) + " " + ratio + ", "
                   + nameof(a) + " " + a + ", "
                   + nameof(b) + " " + b + ", "
                   + base.ToString(_Format, _FormatProvider);
        }
    }

    [Serializable]
    public class TrianglesTexturePropsSet : ReorderableArray<TrianglesTextureProps> { }

    [CreateAssetMenu(fileName = "triangles_texture_set", menuName = "Configs and Sets/Triangles Texture Set")]
    public class TrianglesTexturePropsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public TrianglesTexturePropsSet set;
    }
}
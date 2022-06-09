using System.Collections.Generic;
using Common.Entities;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR
{
    public class RemoteProperties
    {
        public IList<MainColorsProps>         MainColorsSet         { get; set; } = new List<MainColorsProps>();
        public IList<AdditionalColorsProps> BackAndFrontColorsSet { get; set; } = new List<AdditionalColorsProps>();
        public IList<TrianglesTextureProps>   TrianglesTextureSet   { get; set; } = new List<TrianglesTextureProps>();
        public IList<Triangles2TextureProps>  Tria2TextureSet       { get; set; } = new List<Triangles2TextureProps>();
        public string                         TestDeviceIds;
    }
}
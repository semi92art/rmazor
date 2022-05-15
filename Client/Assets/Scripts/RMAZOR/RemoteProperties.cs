using System.Collections.Generic;
using Common.Entities;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR
{
    public class RemoteProperties
    {
        public IList<MainColorsProps>         MainColorsSet         { get; set; } = new List<MainColorsProps>();
        public IList<BackAndFrontColorsProps> BackAndFrontColorsSet { get; set; } = new List<BackAndFrontColorsProps>();
        public IList<TexturePropsBase>        LinesTextureSet       { get; set; } = new List<TexturePropsBase>();
        public IList<TrianglesTextureProps>   TrianglesTextureSet   { get; set; } = new List<TrianglesTextureProps>();
        public IList<Triangles2TextureProps> Tria2TextureSet { get; set; } = new List<Triangles2TextureProps>();
    }
}
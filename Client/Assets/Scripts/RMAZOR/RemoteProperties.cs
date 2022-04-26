using System.Collections.Generic;
using Common.Entities;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR
{
    public class RemoteProperties
    {
        public IList<MainColorsSetItem>         MainColorsSet         { get; set; } = new List<MainColorsSetItem>();
        public IList<BackAndFrontColorsSetItem> BackAndFrontColorsSet { get; set; } = new List<BackAndFrontColorsSetItem>();
        public IList<LinesTextureSetItem>       LinesTextureSet       { get; set; } = new List<LinesTextureSetItem>();
        public IList<CirclesTextureSetItem>     CirclesTextureSet     { get; set; } = new List<CirclesTextureSetItem>();
        public IList<Circles2TextureSetItem>    Circles2TextureSet    { get; set; } = new List<Circles2TextureSetItem>();
        public IList<TrianglesTextureSetItem>   TrianglesTextureSet   { get; set; } = new List<TrianglesTextureSetItem>();
        public IList<Triangles2TextureSetItem>  Triangles2TextureSet  { get; set; } = new List<Triangles2TextureSetItem>();
    }
}
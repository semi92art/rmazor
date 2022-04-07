using System.Collections.Generic;
using Common.Entities;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR
{
    public class RemoteProperties
    {
        public IList<MainColorsSetItem>         MainColorsSet         { get; set; }
        public IList<BackAndFrontColorsSetItem> BackAndFrontColorsSet { get; set; }
        public IList<LinesTextureSetItem>       LinesTextureSet       { get; set; }
        public IList<CirclesTextureSetItem>     CirclesTextureSet     { get; set; }
        public IList<Circles2TextureSetItem>    Circles2TextureSet    { get; set; }
        public IList<TrianglesTextureSetItem>   TrianglesTextureSet   { get; set; }
        public IList<Triangles2TextureSetItem>  Triangles2TextureSet  { get; set; }
    }
}
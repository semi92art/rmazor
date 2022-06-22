using System.Collections.Generic;
using Common.Entities;
using Common.Helpers;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR
{
    public interface IRemotePropertiesRmazor : IRemotePropertiesCommon
    {
        IList<MainColorsProps>        MainColorsSet         { get; set; }
        IList<AdditionalColorsProps>  BackAndFrontColorsSet { get; set; }
        IList<Triangles2TextureProps> Tria2TextureSet       { get; set; }
    }
    
    public class RemotePropertiesRmazor : RemotePropertiesCommon, IRemotePropertiesRmazor 
    {
        public IList<MainColorsProps>        MainColorsSet         { get; set; } = new List<MainColorsProps>();
        public IList<AdditionalColorsProps>  BackAndFrontColorsSet { get; set; } = new List<AdditionalColorsProps>();
        public IList<Triangles2TextureProps> Tria2TextureSet       { get; set; } = new List<Triangles2TextureProps>();
    }
}
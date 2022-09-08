using System.Collections.Generic;
using System.Linq;
using Common.CameraProviders.Camera_Effects_Props;
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
        ColorGradingProps             ColorGradingProps     { get; set; }
    }
    
    public class RemotePropertiesRmazor : RemotePropertiesCommon, IRemotePropertiesRmazor
    {
        private IList<int> m_LevelsInGroup;
        
        public IList<MainColorsProps>        MainColorsSet         { get; set; } = new List<MainColorsProps>();
        public IList<AdditionalColorsProps>  BackAndFrontColorsSet { get; set; } = new List<AdditionalColorsProps>();
        public IList<Triangles2TextureProps> Tria2TextureSet       { get; set; } = new List<Triangles2TextureProps>();

        public ColorGradingProps ColorGradingProps { get; set; }
    }
}
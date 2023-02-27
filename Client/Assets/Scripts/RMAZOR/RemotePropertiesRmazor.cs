using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Helpers;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Helpers;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR
{
    public interface IRemotePropertiesRmazor : IRemotePropertiesCommon
    {
        IList<MainColorsProps>        MainColorsSet         { get; set; }
        IList<AdditionalColorsPropsAssetItem>  BackAndFrontColorsSet { get; set; }
        IList<Triangles2TextureProps> Tria2TextureSet       { get; set; }
        ColorGradingProps             ColorGradingProps     { get; set; }
    }
    
    public class RemotePropertiesRmazor : RemotePropertiesCommon, IRemotePropertiesRmazor
    {
        private IList<int> m_LevelsInGroup;
        
        public IList<MainColorsProps>        MainColorsSet         { get; set; } = new List<MainColorsProps>();
        public IList<AdditionalColorsPropsAssetItem>  BackAndFrontColorsSet { get; set; } = new List<AdditionalColorsPropsAssetItem>();
        public IList<Triangles2TextureProps> Tria2TextureSet       { get; set; } = new List<Triangles2TextureProps>();

        public ColorGradingProps ColorGradingProps { get; set; }
    }
}
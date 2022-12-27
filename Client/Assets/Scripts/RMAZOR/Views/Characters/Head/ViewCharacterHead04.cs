using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHead04 : IViewCharacterHead { }

    public class ViewCharacterHead04 
        : ViewCharacterHeadRectangleBase,
          IViewCharacterHead04
    {
        #region nonpublic members

        protected override string PrefabName => "character_head_04";

        #endregion

        #region inject

        protected ViewCharacterHead04(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner) 
            : base(
                _ViewSettings, 
                _ColorProvider,
                _ContainersGetter, 
                _PrefabSetManager,
                _CoordinateConverter, 
                _AppearTransitioner) { }
        
        #endregion
    }
}
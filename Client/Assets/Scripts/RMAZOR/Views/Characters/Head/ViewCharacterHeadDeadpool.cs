using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Characters.Head
{
    public interface IViewCharacterHeadDeadpool : IViewCharacterHeadOneSprite { }
    
    public class ViewCharacterHeadDeadpool
        : ViewCharacterHeadOneSpriteBase,
          IViewCharacterHeadDeadpool
    {
        #region inject

        private ViewCharacterHeadDeadpool(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner,
            IViewInputCommandsProceeder _CommandsProceeder) 
            : base(
                _ViewSettings,
                _ColorProvider,
                _ContainersGetter,
                _PrefabSetManager, 
                _CoordinateConverter,
                _AppearTransitioner,
                _CommandsProceeder) { }

        #endregion
        
        #region api

        public override string Id => "deadpool";

        #endregion
    }
}
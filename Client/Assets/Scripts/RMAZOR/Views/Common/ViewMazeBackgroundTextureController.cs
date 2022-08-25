using Common.Exceptions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.BackgroundIdleItems;
using RMAZOR.Views.Common.FullscreenTextureProviders;

namespace RMAZOR.Views.Common
{
    public class ViewMazeBackgroundTextureController : ViewMazeBackgroundTextureControllerBase
    {
        #region nonpublic members

        private bool m_IsFirstLoad = true;
        
        #endregion
        
        #region inject

        private IFullscreenTextureProviderParallaxGradientCircles TextureProviderParallaxCircles { get; }
        // private IFullscreenTextureProviderNeonStream              TextureProviderNeonStream      { get; }
        private IFullscreenTextureProviderConnectedDots           TextureProviderConnectedDots   { get; }
        private IFullscreenTextureProviderSwirlForPlanet          TextureProviderSwirlForPlanet  { get; }
        // private IFullscreenTextureProviderWormHole                TextureProviderWormHole        { get; }
        private IViewGameTicker                                   Ticker                         { get; }
        private IViewMazeBackgroundIdleItems                      IdleItems                      { get; }

        private ViewMazeBackgroundTextureController(
            IRemotePropertiesRmazor                           _RemoteProperties,
            IColorProvider                                    _ColorProvider,
            IPrefabSetManager                                 _PrefabSetManager,
            IFullscreenTextureProviderParallaxGradientCircles _TextureProviderParallaxCircles,
            // IFullscreenTextureProviderNeonStream              _TextureProviderNeonStream,
            IFullscreenTextureProviderConnectedDots           _TextureProviderConnectedDots,
            IFullscreenTextureProviderSwirlForPlanet          _TextureProviderSwirlForPlanet,
            // IFullscreenTextureProviderWormHole                _TextureProviderWormHole,
            IViewGameTicker                                   _Ticker,
            IViewMazeBackgroundIdleItems                      _IdleItems) 
            : base(
                _RemoteProperties,
                _ColorProvider, 
                _PrefabSetManager)
        {
            TextureProviderParallaxCircles = _TextureProviderParallaxCircles;
            // TextureProviderNeonStream      = _TextureProviderNeonStream;
            TextureProviderConnectedDots   = _TextureProviderConnectedDots;
            TextureProviderSwirlForPlanet  = _TextureProviderSwirlForPlanet;
            // TextureProviderWormHole        = _TextureProviderWormHole;
            Ticker                         = _Ticker;
            IdleItems                      = _IdleItems;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommonDataRmazor.BackgroundTextureController = this;
            Ticker.Register(this);
            TextureProviderParallaxCircles .Init();
            // TextureProviderNeonStream      .Init();
            TextureProviderConnectedDots   .Init();
            TextureProviderSwirlForPlanet  .Init();
            // TextureProviderWormHole        .Init();
            IdleItems                      .Init();
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            IdleItems.OnLevelStageChanged(_Args);
            if (_Args.LevelStage == ELevelStage.Loaded)
                OnLevelLoaded(_Args);
        }
        
        #if UNITY_EDITOR

        public void SetAdditionalInfo(AdditionalColorPropsAdditionalInfo _AdditionalInfo)
        {
            AdditionalInfo = _AdditionalInfo;
            // TextureProviderNeonStream.SetAdditionalParams(
            //     new TextureProviderNeonStreamAdditionalParams(AdditionalInfo.neonStreamColorCoefficient1));
            TextureProviderSwirlForPlanet.SetAdditionalParams(
                new TextureProviderSwirlForPlanetAdditionalParams(AdditionalInfo.swirlForPlanetColorCoefficient1));
            // TextureProviderWormHole.SetAdditionalParams(
            //     new TextureProviderWormHoleAdditionalParams(AdditionalInfo.wormHoleColorCoefficient1));
        }
        
        #endif

        #endregion
        
        #region nonpublic methods

        private void OnLevelLoaded(LevelStageArgs _Args)
        {
            ActivateConcreteBackgroundTexture(_Args.LevelIndex, out var provider);
            IdleItems.SetSpawnPool(GetProviderIndex(provider));
            int group = RmazorUtils.GetGroupIndex(_Args.LevelIndex);
            long firstLevInGroup = RmazorUtils.GetFirstLevelInGroup(group);
            bool predicate = _Args.LevelIndex == firstLevInGroup || m_IsFirstLoad;
            var colFrom1 = predicate ? BackCol1Current : BackCol1Prev;
            var colFrom2 = predicate ? BackCol2Current : BackCol2Prev;
            m_IsFirstLoad = false;
            provider.Show(
                MathUtils.Epsilon, 
                colFrom1, 
                colFrom2, 
                BackCol1Current, 
                BackCol2Current);
        }

        private void ActivateConcreteBackgroundTexture(long _LevelIndex, out IFullscreenTextureProvider _Provider)
        {
            _Provider = TextureProviderParallaxCircles;
            // TextureProviderNeonStream     .Activate(false);
            TextureProviderParallaxCircles.Activate(false);
            TextureProviderConnectedDots  .Activate(false);
            TextureProviderSwirlForPlanet .Activate(false);
            // TextureProviderWormHole       .Activate(false);
            int group = RmazorUtils.GetGroupIndex(_LevelIndex);
            int c = group % 3;
            switch (c)
            {
                // case 0:
                //     TextureProviderNeonStream.Activate(true);
                //     TextureProviderNeonStream.SetAdditionalParams(
                //         new TextureProviderNeonStreamAdditionalParams(AdditionalInfo.neonStreamColorCoefficient1));
                //     break;
                case 0:  TextureProviderParallaxCircles.Activate(true); break;
                case 1:  TextureProviderConnectedDots  .Activate(true); break;
                case 2:  
                    TextureProviderSwirlForPlanet.Activate(true);
                    TextureProviderSwirlForPlanet.SetAdditionalParams(
                        new TextureProviderSwirlForPlanetAdditionalParams(AdditionalInfo.swirlForPlanetColorCoefficient1));
                    break;
                // case 4: 
                //     TextureProviderWormHole.Activate(true); 
                //     TextureProviderWormHole.SetAdditionalParams(
                //         new TextureProviderWormHoleAdditionalParams(AdditionalInfo.wormHoleColorCoefficient1));
                //     break;
                default: throw new SwitchCaseNotImplementedException(c);
            }
        }

        private static int GetProviderIndex(IFullscreenTextureProvider _Provider)
        {
            return _Provider switch
            {
                IFullscreenTextureProviderParallaxGradientCircles => 1,
                IFullscreenTextureProviderNeonStream              => 1,
                IFullscreenTextureProviderConnectedDots           => 1,
                IFullscreenTextureProviderSwirlForPlanet          => 1,
                IFullscreenTextureProviderWormHole                => 1,
                _ => throw new SwitchCaseNotImplementedException(_Provider)
            };
        }

        #endregion
    }
}
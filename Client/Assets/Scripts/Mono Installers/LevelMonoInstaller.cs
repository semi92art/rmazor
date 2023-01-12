using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using RMAZOR;
using RMAZOR.Controllers;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.UI.Panels;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.Views;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Characters.Head;
using RMAZOR.Views.Characters.Legs;
using RMAZOR.Views.Characters.Tails;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Common.BackgroundIdleItems;
using RMAZOR.Views.Common.CongratulationItems;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.Common.ViewLevelStageController;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Common.ViewUILevelSkippers;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.MazeItems.ViewMazeItemPath;
using RMAZOR.Views.MazeItems.ViewMazeItemPath.ExtraBorders;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstallerImplBase
    {
        public override void InstallBindings()
        {
            base.InstallBindings();
            BindView();
            BindOther();
        }
        
        private void BindView()
        {
            BindViewCommon();
            BindUiCommon();
            BindUiPanelsAndViewers();
            BindMazeItemBlocksAndGroups();
            BindTextureProviders();
            BindCharacter();
        }

        private void BindViewCommon()
        {

            Container.Bind<IMazeShaker>()                      .To<MazeShaker>()                      .AsSingle();
            Container.Bind<IMazeItemsCreator>()                .To<MazeItemsCreator>()                .AsSingle();
            Container.Bind<IMoneyItemsOnPathItemsDistributor>().To<MoneyItemsOnPathItemsDistributor>().AsSingle();
            Container.Bind<IViewGame>()                        .To<ViewGame>()                        .AsSingle();
            Container.Bind<IViewMazeCommon>()                  .To<ViewMazeCommon>()                  .AsSingle();
            Container.Bind<IViewMazeRotation>()                .To<ViewMazeRotation>()                .AsSingle();
            Container.Bind<IViewBackground>()                  .To<ViewBackground>()                  .AsSingle();
            Container.Bind<IViewMazeForeground>()              .To<ViewMazeForeground>()              .AsSingle();
            Container.Bind<IViewInputTouchProceeder>()         .To<ViewInputTouchProceeder>()         .AsSingle();
            
            Container.Bind<IViewMazeBackgroundIdleItemDisc>()    .To<ViewMazeBackgroundIdleItemDisc>()    .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemSquare>()  .To<ViewMazeBackgroundIdleItemSquare>()  .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemTriangle>().To<ViewMazeBackgroundIdleItemTriangle>().AsSingle();
            Container.Bind<IViewMazeBackgroundCongratItems>()    .To<ViewMazeBackgroundCongratItems2>()   .AsSingle();
            Container.Bind<IRendererAppearTransitioner>()        .To<RendererAppearTransitioner>()        .AsSingle();
            Container.Bind<IViewFullscreenTransitioner>()        .To<ViewFullscreenTransitioner>()        .AsSingle();
            Container.Bind<IViewCameraEffectsCustomAnimator>()   .To<ViewCameraEffectsCustomAnimator>()   .AsSingle();
            Container.Bind<IViewLevelStageController>()          .To<ViewLevelStageController>()          .AsSingle();
            
            Container.Bind<IViewLevelStageControllerOnLevelLoaded>()     
                .To<ViewLevelStageControllerOnLevelLoaded>()  
                .AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelReadyToStart>()
                .To<ViewLevelStageControllerOnLevelReadyToStart>()
                .AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelFinished>()  
                .To<ViewLevelStageControllerOnLevelFinished>()
                .AsSingle();
            Container.Bind<IViewLevelStageControllerOnReadyToUnloadLevel>()
                .To<ViewLevelStageControllerOnReadyToUnloadLevel>()
                .AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelUnloaded>()  
                .To<ViewLevelStageControllerOnLevelUnloaded>()
                .AsSingle();
            Container.Bind<IViewLevelStageControllerOnCharacterKilled>()
                .To<ViewLevelStageControllerOnCharacterKilled>()
                .AsSingle();
            Container.Bind<IViewLevelStageControllerOnExitLevelStaging>()
                .To<ViewLevelStageControllerOnExitLevelStaging>()
                .AsSingle();
            
            Container.Bind<IViewMazeAdditionalBackground>()      
                .To<ViewMazeAdditionalBackground>()   
                // .To<ViewMazeAdditionalBackgroundFake>()   
                .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItems>()      
                .To<ViewMazeBackgroundIdleItems>()    
                // .To<ViewMazeBackgroundIdleItemsFake>()    
                .AsSingle();
            
            Container.Bind<IViewMazeAdditionalBackgroundGeometryInitializer>()
                .To<ViewMazeAdditionalBackgroundGeometryInitializerSimple>()
                .AsSingle();
            Container.Bind<IViewMazeAdditionalBackgroundDrawer>()
                // .To<ViewMazeAdditionalBackgroundDrawerRmazor>()
                .To<ViewMazeAdditionalBackgroundDrawerRmazorOnlyMask>()
                .AsSingle();
            // Container.Bind<IViewMazeAdditionalBackgroundDrawerRmazorFull>()
            //     .To<ViewMazeAdditionalBackgroundDrawerRmazorFull>()
            //     .AsSingle();
            // Container.Bind<IViewMazeAdditionalBackgroundDrawerRmazorOnlyMask>()
            //     .To<ViewMazeAdditionalBackgroundDrawerRmazorOnlyMask>()
            //     .AsSingle();
            

            Container.Bind<IViewSwitchLevelStageCommandInvoker>()
                .To<ViewSwitchLevelStageCommandInvoker>()
                .AsSingle();
        }

        private void BindMazeItemBlocksAndGroups()
        {
            Container.Bind<IViewMazeItemPathExtraBorders2>()  .To<ViewMazeItemPathExtraBorders2>()  .AsSingle();
            Container.Bind<IViewMazeItemPathExtraBorders1>()  .To<ViewMazeItemPathExtraBorders1>()  .AsSingle();
            Container.Bind<IViewMazeItemPathExtraBorders3>()  .To<ViewMazeItemPathExtraBorders3>()  .AsSingle();
            Container.Bind<IViewMazeItemPathExtraBordersSet>().To<ViewMazeItemPathExtraBordersSet>().AsSingle();
            Container.Bind<IViewMazeItemsPathInformer>()      .To<ViewMazeItemsPathInformer>()      .AsTransient();
            switch (CommonData.GameId)
            {
                case GameIds.RMAZOR:
                    Container.Bind<IViewMazeItemPath>().To<ViewMazeItemPathRmazor>().AsSingle();
                    break;
                case GameIds.ZMAZOR:
                    Container.Bind<IViewMazeItemPath>().To<ViewMazeItemPathRmazor>().AsSingle();
                    break;
            }
            Container.Bind<IViewMazeItemPathItem>()          .To<ViewMazeItemPathItem>()            .AsSingle();
            Container.Bind<IViewMazeItemPathItemMoney>()     .To<ViewMazeItemPathItemMoneyRectangle>()   .AsSingle();
            Container.Bind<IViewMazeItemPathItemIdle>()      .To<ViewMazeItemPathItemIdleSquare>()  .AsSingle();
            
            Container.Bind<IViewMazeItemGravityBlock>()      .To<ViewMazeItemGravityBlock>()         .AsSingle();
            Container.Bind<IViewMazeItemMovingTrap>()        .To<ViewMazeItemMovingTrap>()           .AsSingle();
            Container.Bind<IViewMazeItemShredingerBlock>()   .To<ViewMazeItemShredingerBlock>()      .AsSingle();
            Container.Bind<IViewMazeItemTurret>()            .To<ViewMazeItemTurret>()               .AsSingle();
            Container.Bind<IViewMazeItemSpringboard>()       .To<ViewMazeItemSpringboard>()          .AsSingle();
            Container.Bind<IViewMazeItemPortal>()            .To<ViewMazeItemPortal>()               .AsSingle();
            Container.Bind<IViewMazeItemGravityTrap>()       .To<ViewMazeItemGravityTrap>()          .AsSingle();
            Container.Bind<IViewMazeItemTrapReact>()         .To<ViewMazeItemTrapReactSpikes>()      .AsSingle();
            Container.Bind<IViewMazeItemTrapIncreasing>()    .To<ViewMazeItemTrapIncreasingSickles>().AsSingle();
            Container.Bind<IViewMazeItemGravityBlockFree>()  .To<ViewMazeItemGravityBlockFree>()     .AsSingle();
            Container.Bind<IViewMazeItemHammer>()            .To<ViewMazeItemHammer>()               .AsSingle();
            Container.Bind<IViewMazeItemSpear>()             .To<ViewMazeItemSpear>()                .AsSingle();
            Container.Bind<IViewMazeItemDiode>()             .To<ViewMazeItemDiode>()                .AsSingle();
            Container.Bind<IViewMazeItemKeyLock>()           .To<ViewMazeItemKeyLock>()              .AsSingle();
            
            Container.Bind<IViewMazePathItemsGroup>()        .To<ViewMazePathItemsGroupRmazor>()     .AsSingle();
            Container.Bind<IViewMazeMovingItemsGroup>()      .To<ViewMazeMovingItemsGroup>()         .AsSingle();
            Container.Bind<IViewMazeShredingerBlocksGroup>() .To<ViewMazeShredingerBlocksGroup>()    .AsSingle();
            Container.Bind<IViewMazeTurretsGroup>()          .To<ViewMazeTurretsGroup>()             .AsSingle();
            Container.Bind<IViewMazeSpringboardsGroup>()     .To<ViewMazeSpringboardsGroup>()        .AsSingle();
            Container.Bind<IViewMazePortalsGroup>()          .To<ViewMazePortalsGroup>()             .AsSingle();
            Container.Bind<IViewMazeTrapsReactItemsGroup>()  .To<ViewMazeTrapsReactSpikesItemGroup>().AsSingle();
            Container.Bind<IViewMazeTrapsIncItemsGroup>()    .To<ViewMazeTrapsIncItemsGroup>()       .AsSingle();
            Container.Bind<IViewMazeGravityItemsGroup>()     .To<ViewMazeGravityItemsGroup>()        .AsSingle();
            Container.Bind<IViewMazeHammersGroup>()          .To<ViewMazeHammersGroup>()             .AsSingle();
            Container.Bind<IViewMazeSpearsGroup>()           .To<ViewMazeSpearsGroup>()              .AsSingle();
            Container.Bind<IViewMazeDiodesGroup>()           .To<ViewMazeDiodesGroup>()              .AsSingle();
            Container.Bind<IViewMazeKeyLockGroup>()          .To<ViewMazeKeyLockGroup>()             .AsSingle();
            
            Container.Bind<IViewMazeItemsGroupSet>()         .To<ViewMazeItemsGroupSet>()            .AsSingle();
            
            Container.Bind<IViewTurretProjectile>()          .To<ViewTurretProjectileShuriken>()     .AsSingle();
            Container.Bind<IViewTurretProjectileTail>()      .To<ViewTurretProjectileTail>()         .AsSingle();
            Container.Bind<IViewTurretProjectileTailFake>()  .To<ViewTurretProjectileTailFake>()     .AsSingle();
            Container.Bind<IViewTurretBody>()                .To<ViewTurretBodySquare>()             .AsSingle();
        }

        private void BindCharacter()
        {
            Container.Bind<IViewCharacter>()         .To<ViewCharacter>()                 .AsSingle();
            Container.Bind<IViewCharacterEffector>() .To<ViewCharacterEffectorParticles>().AsSingle();
            Container.Bind<IViewParticleBubble>()    .To<ViewParticleBubble>()            .AsSingle();
            Container.Bind<IViewParticleSpark>()     .To<ViewParticleSpark>()             .AsSingle();
            Container.Bind<IViewParticlesThrower>()  .To<ViewParticlesThrower>()          .AsTransient();

            Container.Bind<IViewCharactersSet>().To<ViewCharactersSet>().AsSingle();

            Container.Bind<IViewCharacterLegs01>().To<ViewCharacterLegs01>().AsSingle();
            Container.Bind<IViewCharacterLegsFake>().To<ViewCharacterLegsFake>().AsSingle();

            Container.Bind<IViewCharacterTail01>().To<ViewCharacterTail01>().AsSingle();
            Container.Bind<IViewCharacterTailFake>().To<ViewCharacterTailFake>().AsSingle();
            
            Container.Bind<IViewCharacterHead01>().To<ViewCharacterHead01>().AsSingle();
            Container.Bind<IViewCharacterHead02>().To<ViewCharacterHead02>().AsSingle();
            // Container.Bind<IViewCharacterHead03>().To<ViewCharacterHead03>().AsSingle();
            // Container.Bind<IViewCharacterHead04>().To<ViewCharacterHead04>().AsSingle();
            // Container.Bind<IViewCharacterHead05>().To<ViewCharacterHead05>().AsSingle();
            // Container.Bind<IViewCharacterHead06>().To<ViewCharacterHead06>().AsSingle();
            // Container.Bind<IViewCharacterHead07>().To<ViewCharacterHead07>().AsSingle();
            // Container.Bind<IViewCharacterHead08>().To<ViewCharacterHead08>().AsSingle();
        }

        private void BindUiCommon()
        {
            Container.Bind<IViewUICanvasGetter>()       .To<ViewUICanvasGetter>()     .AsSingle();
            if (!MazorCommonData.Release)
            {
                Container.Bind<IViewUI>()               .To<ViewUIFake>()             .AsSingle();
                Container.Bind<IViewUIGameControls>()   .To<ViewUIGameControlsFake>() .AsSingle();
                Container.Bind<IViewUILevelSkipper>()   .To<ViewUILevelSkipperFake>() .AsSingle();
            }
            else
            {
                Container.Bind<IViewUI>()               .To<ViewUI>()                 .AsSingle();
                Container.Bind<IViewUIGameControls>()   .To<ViewUIGameControls>()     .AsSingle();
                Container.Bind<IViewUIPrompt>()         .To<ViewUIPrompt>()           .AsSingle();
                Container.Bind<IViewUICongratsMessage>().To<ViewUICongratsMessage>()  .AsSingle();
                Container.Bind<IViewUILevelsPanel>()    .To<ViewUILevelsPanel>()      .AsSingle();
                Container.Bind<IViewUITopButtons>()     .To<ViewUITopButtons>()       .AsSingle();
                Container.Bind<IViewUITutorial>()       .To<ViewUITutorial>()         .AsSingle();
                
                Container.Bind<IViewUIRateGamePanelController>()
                    // .To<ViewUIRateGamePanelController>()
                    .To<ViewUIRateGamePanelControllerFake>()
                    .AsSingle();
                Container.Bind<IViewUIStarsAndTimePanel>()
                    // .To<ViewUIStarsAndTimePanel>()
                    .To<ViewUIStarsAndTimePanelFake>()  
                    .AsSingle();
                Container.Bind<IViewUILevelSkipper>()
                    .To<ViewUILevelSkipperButton>()
                    // .To<ViewUILevelSkipperFake>()
                    .AsSingle();
                Container.Bind<IViewUIRotationControls>()
                    .To<ViewUIRotationControlsButtons>()
                    // .To<ViewUIRotationControlsFake>()
                    .AsSingle();
            }
        }

        private void BindUiPanelsAndViewers()
        {
            if (!MazorCommonData.Release)
            {
                Container.Bind<IDialogViewersController>()    .To<DialogViewersControllerFake>()    .AsSingle();
                Container.Bind<IDialogViewerMediumCommon>()   .To<DialogViewerMediumCommonFake>()   .AsSingle();
                Container.Bind<IDialogViewerMedium2>()        .To<DialogViewerMedium2Fake>()        .AsSingle();
                Container.Bind<IDialogViewerFullscreenCommon>().To<DialogViewerFullscreenCommonFake>().AsSingle();
                Container.Bind<IDialogViewerFullscreen2>()    .To<DialogViewerFullscreen2Fake>()    .AsSingle();
                Container.Bind<IDialogPanelsSet>()            .To<DialogPanelsSetFake>()            .AsSingle();
                Container.Bind<IRateGameDialogPanel>()        .To<RateGameDialogPanelFake>()        .AsSingle();
                Container.Bind<IFinishLevelGroupDialogPanel>().To<FinishLevelGroupDialogPanelFake>().AsSingle();
                Container.Bind<IPlayBonusLevelDialogPanel>()  .To<PlayBonusLevelDialogPanelFake>()  .AsSingle();
                Container.Bind<IDailyGiftPanel>()             .To<DailyGiftPanelFake>()             .AsSingle();
                Container.Bind<ILevelsDialogPanel>()          .To<LevelsDialogPanelFake>()          .AsSingle();
                Container.Bind<IConfirmLoadLevelDialogPanel>().To<ConfirmLoadLevelDialogPanelFake>().AsSingle();
                Container.Bind<IMainMenuPanel>()              .To<MainMenuPanelFake>()              .AsSingle();
            }
            else
            {
                Container.Bind<IDialogViewersController>()    .To<DialogViewersController>()        .AsSingle();
                Container.Bind<IDialogViewerMediumCommon>()   .To<DialogViewerMediumCommon>()       .AsSingle();
                Container.Bind<IDialogViewerMedium2>()        .To<DialogViewerMedium2>()            .AsSingle();
                Container.Bind<IDialogViewerFullscreenCommon>().To<DialogViewerFullscreenCommon>()  .AsSingle();
                Container.Bind<IDialogViewerFullscreen2>()    .To<DialogViewerFullscreen2>()        .AsSingle();
                Container.Bind<IDialogPanelsSet>()            .To<DialogPanelsSet>()                .AsSingle();
                
                Container.Bind<IRateGameDialogPanel>()        .To<RateGameDialogPanel>()            .AsSingle();
                Container.Bind<ITutorialDialogPanel>()        .To<TutorialDialogPanel>()            .AsSingle();
                Container.Bind<ISettingLanguageDialogPanel>() .To<SettingsLanguagePanel>()          .AsSingle();
                Container.Bind<IShopDialogPanel>()            .To<ShopDialogPanel>()                .AsSingle();
                Container.Bind<IDisableAdsDialogPanel>()      .To<DisableAdsDialogPanel>()          .AsSingle();
                Container.Bind<ISettingDialogPanel>()         .To<SettingsDialogPanel>()            .AsSingle();
                Container.Bind<ICharacterDiedDialogPanel>()   .To<CharacterDiedDialogPanel>()       .AsSingle();
                Container.Bind<IFinishLevelGroupDialogPanel>().To<FinishLevelGroupDialogPanel>()    .AsSingle();
                Container.Bind<IPlayBonusLevelDialogPanel>()  .To<PlayBonusLevelDialogPanel>()      .AsSingle();
                Container.Bind<IDailyGiftPanel>()             .To<DailyGiftPanel>()                 .AsSingle();
                Container.Bind<ILevelsDialogPanel>()          .To<LevelsDialogPanel>()              .AsSingle();
                Container.Bind<IConfirmLoadLevelDialogPanel>().To<ConfirmLoadLevelDialogPanel>()    .AsSingle();
                Container.Bind<IMainMenuPanel>()              .To<MainMenuPanel>()                  .AsSingle();
            }
        }

        private void BindTextureProviders()
        {
            Container.Bind<IFullscreenTextureProviderCustom>()
                .To<FullscreenTextureProviderCustom>()
                .AsSingle();
            Container.Bind<IFullscreenTextureProviderTriangles2>()
                .To<FullscreenTextureProviderTriangles2>()
                .AsSingle();
            Container.Bind<IFullscreenTextureProviderEmpty>()
                .To<FullscreenTextureProviderEmpty>()
                .AsSingle();
            Container.Bind<IFullscreenTransitionTextureProviderCircles>()
                .To<FullscreenTransitionTextureProviderCircles>()
                .AsSingle();
            Container.Bind<IFullscreenTransitionTextureProvider>()
                .To<FullscreenTransitionTextureProviderPlayground>()
                .AsSingle();
            Container.Bind<IBackgroundTextureProviderSet>()
                .To<BackgroundTextureProviderSetImpl>()
                .AsSingle();
            Container.Bind<IViewMazeBackgroundTextureController>()
                .To<ViewMazeBackgroundTextureControllerRmazor>()
                .AsSingle();
        }
        
        private void BindOther()
        {
            Container.Bind<IRawLevelInfoGetter>()           .To<RawLevelInfoGetter>()           .AsSingle();
            Container.Bind<IGameController>()               .To<GameControllerMVC>()            .AsSingle();
            Container.Bind<IDebugManager>()                 .To<DebugManager>()                 .AsSingle();
            Container.Bind<IManagersGetter>()               .To<ManagersGetter>()               .AsSingle();
            Container.Bind<IViewBetweenLevelAdShower>()     .To<ViewBetweenLevelAdShower>()     .AsSingle();
            Container.Bind<IMoneyCounter>()                 .To<MoneyCounter>()                 .AsSingle();
            Container.Bind<IViewTimePauser>()               .To<ViewTimePauser>()               .AsSingle();
            Container.Bind<IViewIdleAdsPlayer>()            .To<ViewIdleAdsPlayer>()            .AsSingle();
            Container.Bind<IViewGameIdleQuitter>()          .To<ViewGameIdleQuitter>()          .AsSingle();
            Container.Bind<IViewMobileNotificationsSender>().To<ViewMobileNotificationsSender>().AsSingle();

            Container.Bind(typeof(IAudioManagerRmazor), typeof(IAudioManager))
                .To<AudioManagerRmazor>()
                .AsSingle();
        }
    }
}

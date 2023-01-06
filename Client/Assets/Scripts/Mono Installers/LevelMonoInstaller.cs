using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using RMAZOR;
using RMAZOR.Camera_Providers;
using RMAZOR.Controllers;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.InputSchedulers;
using RMAZOR.Models.ItemProceeders;
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
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.MazeItems.ViewMazeItemPath;
using RMAZOR.Views.MazeItems.ViewMazeItemPath.ExtraBorders;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using RMAZOR.Views.UI.Game_Logo;
using UnityEngine;
using ZMAZOR.Views.Camera_Providers;
using ZMAZOR.Views.Coordinate_Converters;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstallerImplBase
    {
        public GameObject colorProvider;

        public override void InstallBindings()
        {
            base.InstallBindings();
            BindModel();
            BindView();
            BindCamera();
            BindOther();
        }

        private void BindModel()
        {
            Container.Bind<IInputScheduler>()             .To<InputScheduler>()             .AsSingle();
            Container.Bind<IInputSchedulerGameProceeder>().To<InputSchedulerGameProceeder>().AsSingle();
            Container.Bind<IInputSchedulerUiProceeder>()  .To<InputSchedulerUiProceeder>()  .AsSingle();
            Container.Bind<IModelData>()                  .To<ModelData>()                  .AsSingle();
            Container.Bind<IModelMazeRotation>()          .To<ModelMazeRotation>()          .AsSingle();
            Container.Bind<IModelCharacter>()             .To<ModelCharacter>()             .AsSingle();
            Container.Bind<IModelGame>()                  .To<ModelGame>()                  .AsSingle();
            Container.Bind<IModelLevelStaging>()          .To<ModelLevelStaging>()          .AsSingle();
            Container.Bind<IPathItemsProceeder>()         .To<PathItemsProceeder>()         .AsSingle();
            Container.Bind<ITrapsMovingProceeder>()       .To<TrapsMovingProceeder>()       .AsSingle();
            Container.Bind<IGravityItemsProceeder>()      .To<GravityItemsProceeder>()      .AsSingle();
            Container.Bind<ITrapsReactProceeder>()        .To<TrapsReactProceeder>()        .AsSingle();
            Container.Bind<ITurretsProceeder>()           .To<TurretsProceeder>()           .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()   .To<TrapsIncreasingProceeder>()   .AsSingle();
            Container.Bind<IPortalsProceeder>()           .To<PortalsProceeder>()           .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()  .To<ShredingerBlocksProceeder>()  .AsSingle();
            Container.Bind<ISpringboardProceeder>()       .To<SpringboardProceeder>()       .AsSingle();
            Container.Bind<IHammersProceeder>()           .To<HammersProceeder>()           .AsSingle();
            Container.Bind<ISpearsProceeder>()            .To<SpearsProceeder>()            .AsSingle();
            Container.Bind<IDiodesProceeder>()            .To<DiodesProceeder>()            .AsSingle();
            Container.Bind<IKeyLockMazeItemsProceeder>()  .To<KeyLockMazeItemsProceeder>()  .AsSingle();
            Container.Bind<IModelItemsProceedersSet>()    .To<ModelItemsProceedersSet>()    .AsSingle();
            
            Container.Bind<IModelMazeInfoCorrector>()         
                .To<ModelMazeInfoCorrectorWithWallSurrounding>()
                .AsSingle();
            Container.Bind<IModelCharacterPositionValidator>()
                .To<ModelCharacterPositionValidator>()
                .AsSingle();
            Container.Bind<IRevivePositionProvider>()
                .To<RevivePositionProvider>()
                .AsSingle();
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
            Container.Bind<ICoordinateConverterForSmallLevels>().To<CoordinateConverterForSmallLevels>().AsSingle();
            Container.Bind<ICoordinateConverterForBigLevels>()  .To<CoordinateConverterForBigLevels>()  .AsSingle();
            switch (CommonData.GameId)
            {
                case GameIds.RMAZOR:
                    Container.Bind<ICoordinateConverter>().To<CoordinateConverterRmazor>().AsSingle();
                    break;
                case GameIds.ZMAZOR:
                    Container.Bind<ICoordinateConverter>().To<CoordinateConverterZmazor>().AsSingle();
                    break;
            }
            Container.Bind(typeof(IContainersGetter), typeof(IContainersGetterRmazor))
                .To<ContainersGetterRmazor>()
                .AsSingle();
            Container.Bind<IMazeShaker>()                      .To<MazeShaker>()                      .AsSingle();
            Container.Bind<IMazeItemsCreator>()                .To<MazeItemsCreator>()                .AsSingle();
            Container.Bind<IMoneyItemsOnPathItemsDistributor>().To<MoneyItemsOnPathItemsDistributor>().AsSingle();
            Container.Bind<IViewGame>()                        .To<ViewGame>()                        .AsSingle();
            Container.Bind<IViewMazeCommon>()                  .To<ViewMazeCommon>()                  .AsSingle();
            Container.Bind<IViewMazeRotation>()                .To<ViewMazeRotation>()                .AsSingle();
            Container.Bind<IViewBackground>()                  .To<ViewBackground>()                  .AsSingle();
            Container.Bind<IViewMazeForeground>()              .To<ViewMazeForeground>()              .AsSingle();
            Container.Bind<IViewInputTouchProceeder>()         .To<ViewInputTouchProceeder>()         .AsSingle();

            Container.Bind<IViewMazeAdditionalBackground>()      
                .To<ViewMazeAdditionalBackground>()   
                // .To<ViewMazeAdditionalBackgroundFake>()   
                .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItems>()      
                .To<ViewMazeBackgroundIdleItems>()    
                // .To<ViewMazeBackgroundIdleItemsFake>()    
                .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemDisc>()    .To<ViewMazeBackgroundIdleItemDisc>()    .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemSquare>()  .To<ViewMazeBackgroundIdleItemSquare>()  .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemTriangle>().To<ViewMazeBackgroundIdleItemTriangle>().AsSingle();
            Container.Bind<IViewMazeBackgroundCongratItems>()    .To<ViewMazeBackgroundCongratItems2>()   .AsSingle();
            Container.Bind<IRendererAppearTransitioner>()        .To<RendererAppearTransitioner>()        .AsSingle();
            Container.Bind<IViewFullscreenTransitioner>()        .To<ViewFullscreenTransitioner>()        .AsSingle();
            Container.Bind<IViewLevelStageController>()          .To<ViewLevelStageController>()          .AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelLoaded>()       .To<ViewLevelStageControllerOnLevelLoaded>().AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelReadyToStart>() .To<ViewLevelStageControllerOnLevelReadyToStart>().AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelFinished>()     .To<ViewLevelStageControllerOnLevelFinished>().AsSingle();
            Container.Bind<IViewLevelStageControllerOnReadyToUnloadLevel>().To<ViewLevelStageControllerOnReadyToUnloadLevel>().AsSingle();
            Container.Bind<IViewLevelStageControllerOnLevelUnloaded>()     .To<ViewLevelStageControllerOnLevelUnloaded>().AsSingle();
            Container.Bind<IViewLevelStageControllerOnCharacterKilled>()   .To<ViewLevelStageControllerOnCharacterKilled>().AsSingle();
            Container.Bind<IViewCameraEffectsCustomAnimator>()   .To<ViewCameraEffectsCustomAnimator>()   .AsSingle();
            Container.Bind<IRotatingPossibilityIndicator>()      .To<RotatingPossibilityIndicator>()      .AsTransient();
            
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
            
            var inputControllerType = Application.isEditor ? 
                typeof(ViewCommandsProceederInEditor) : typeof(ViewInputCommandsProceeder);
            Container.Bind<IViewInputCommandsProceeder>()
                .To(inputControllerType) 
                .AsSingle();
            Container.Bind<IColorProvider>()             
                .FromComponentInNewPrefab(colorProvider)
                .AsSingle();
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
            Container.Bind<IViewUICanvasGetter>()        .To<ViewUICanvasGetter>()            .AsSingle();
            if (!MazorCommonData.Release)
            {
                Container.Bind<IViewUI>()                .To<ViewUIFake>()                    .AsSingle();
                Container.Bind<IViewUIGameControls>()    .To<ViewUIGameControlsFake>()        .AsSingle();
                Container.Bind<IViewUIGameLogo>()        .To<ViewUIGameLogoFake>()            .AsSingle();
                Container.Bind<IViewUILevelSkipper>()    .To<ViewUILevelSkipperFake>()        .AsSingle();
            }
            else
            {
                Container.Bind<IViewUI>()                .To<ViewUI>()                        .AsSingle();
                Container.Bind<IViewUIRateGamePanelController>().To<ViewUIRateGamePanelController>().AsSingle();
                Container.Bind<IViewUIGameControls>()    .To<ViewUIGameControls>()            .AsSingle();
                Container.Bind<IViewUIPrompt>()          .To<ViewUIPrompt>()                  .AsSingle();
                Container.Bind<IViewUICongratsMessage>() .To<ViewUICongratsMessage>()         .AsSingle();
                Container.Bind<IViewUIGameLogo>()        .To<ViewUIGameLogoDyeBall>()         .AsSingle();
                Container.Bind<IViewUILevelsPanel>()     .To<ViewUILevelsPanel>()             .AsSingle();
                Container.Bind<IViewUITopButtons>()      .To<ViewUITopButtons>()              .AsSingle();
                Container.Bind<IViewUIStarsAndTimePanel>().To<ViewUIStarsAndTimePanel>()      .AsSingle();
                Container.Bind<IViewUITutorial>()        .To<ViewUITutorial>()                .AsSingle();
                
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
                Container.Bind<IDialogViewerMedium1>()        .To<DialogViewerMedium1Fake>()        .AsSingle();
                Container.Bind<IDialogViewerMedium2>()        .To<DialogViewerMedium2Fake>()        .AsSingle();
                Container.Bind<IDialogViewerMedium3>()        .To<DialogViewerMedium3Fake>()        .AsSingle();
                Container.Bind<IDialogPanelsSet>()            .To<DialogPanelsSetFake>()            .AsSingle();
                Container.Bind<IRateGameDialogPanel>()        .To<RateGameDialogPanelFake>()        .AsSingle();
                Container.Bind<IFinishLevelGroupDialogPanel>().To<FinishLevelGroupDialogPanelFake>().AsSingle();
                Container.Bind<IPlayBonusLevelDialogPanel>()  .To<PlayBonusLevelDialogPanelFake>()  .AsSingle();
                Container.Bind<IDailyGiftPanel>()             .To<DailyGiftPanelFake>()             .AsSingle();
                Container.Bind<ILevelsDialogPanel>()          .To<LevelsDialogPanelFake>()          .AsSingle();
                Container.Bind<IConfirmLoadLevelDialogPanel>().To<ConfirmLoadLevelDialogPanelFake>().AsSingle();
            }
            else
            {
                Container.Bind<IDialogViewersController>()    .To<DialogViewersController>()        .AsSingle();
                Container.Bind<IDialogViewerMedium1>()        .To<DialogViewerMedium1>()            .AsSingle();
                Container.Bind<IDialogViewerMedium2>()        .To<DialogViewerMedium2>()            .AsSingle();
                Container.Bind<IDialogViewerMedium3>()        .To<DialogViewerMedium3>()            .AsSingle();
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
            Container.Bind<IViewMazeGameLogoTextureProvider>()   
                .To<ViewMazeGameLogoTextureProviderCirclesToSquares>()
                .AsSingle();
            Container.Bind<IViewMazeBackgroundTextureController>()
                .To<ViewMazeBackgroundTextureController>()
                .AsSingle();
        }

        private void BindCamera()
        {
            Container.Bind<IStaticCameraProvider>() .To<StaticCameraProvider>() .AsSingle();
            Container.Bind<IDynamicCameraProvider>().To<DynamicCameraProvider>().AsSingle();
            Container.Bind<ICameraProviderFake>()   .To<CameraProviderFake>()   .AsSingle();
            switch (CommonData.GameId)
            {
                case GameIds.RMAZOR:
                    Container.Bind<ICameraProvider>().To<CameraProviderRmazor>().AsSingle();
                    break;
                case GameIds.ZMAZOR:
                    Container.Bind<ICameraProvider>().To<CameraProviderZmazor>().AsSingle();
                    break;
            }
        }
        
        private void BindOther()
        {
            Container.Bind<IRawLevelInfoGetter>()      .To<RawLevelInfoGetter>()      .AsSingle();
            Container.Bind<IGameController>()          .To<GameControllerMVC>()       .AsSingle();
            Container.Bind<IDebugManager>()            .To<DebugManager>()            .AsSingle();
            Container.Bind<IManagersGetter>()          .To<ManagersGetter>()          .AsSingle();
            Container.Bind<IViewBetweenLevelAdShower>().To<ViewBetweenLevelAdShower>().AsSingle();
            Container.Bind<IMoneyCounter>()            .To<MoneyCounter>()            .AsSingle();
            Container.Bind<IViewTimePauser>()          .To<ViewTimePauser>()          .AsSingle();

            Container.Bind(typeof(IAudioManagerRmazor), typeof(IAudioManager))
                .To<AudioManagerRmazor>()
                .AsSingle();
        }
    }
}

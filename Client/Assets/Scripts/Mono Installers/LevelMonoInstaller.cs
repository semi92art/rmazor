using Common;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.UI;
using RMAZOR;
using RMAZOR.Controllers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.InputSchedulers;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.UI.Panels;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.Views;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.BackgroundIdleItems;
using RMAZOR.Views.Common.CongratulationItems;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using RMAZOR.Views.UI.Game_Logo;
using UnityEngine;
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
            Container.Bind<IModelItemsProceedersSet>()    .To<ModelItemsProceedersSet>()    .AsSingle();
            
            Container.Bind<IModelMazeInfoCorrector>()     .To<ModelMazeInfoCorrectorWithWallSurrounding>().AsSingle();
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
            Container.Bind<IMazeShaker>()             .To<MazeShaker>()             .AsSingle();
            Container.Bind<IMazeItemsCreator>()       .To<MazeItemsCreator>()       .AsSingle();
            Container.Bind<IViewGame>()               .To<ViewGame>()               .AsSingle();
            Container.Bind<IViewMazeCommon>()         .To<ViewMazeCommon>()         .AsSingle();
            Container.Bind<IViewMazeRotation>()       .To<ViewMazeRotation>()       .AsSingle();
            Container.Bind<IViewBackground>()         .To<ViewBackground>()         .AsSingle();
            Container.Bind<IViewMazeForeground>()     .To<ViewMazeForeground>()     .AsSingle();
            Container.Bind<IViewInputTouchProceeder>().To<ViewInputTouchProceeder>().AsSingle();

            Container.Bind<IViewMazeAdditionalBackground>()      .To<ViewMazeAdditionalBackground>()      .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItems>()       .To<ViewMazeBackgroundIdleItems>()       .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemDisc>()    .To<ViewMazeBackgroundIdleItemDisc>()    .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemSquare>()  .To<ViewMazeBackgroundIdleItemSquare>()  .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItemTriangle>().To<ViewMazeBackgroundIdleItemTriangle>().AsSingle();
            Container.Bind<IViewMazeBackgroundCongradItems>()    .To<ViewMazeBackgroundCongradItems2>()   .AsSingle();
            Container.Bind<IRendererAppearTransitioner>()        .To<RendererAppearTransitioner>()        .AsSingle();
            Container.Bind<IViewFullscreenTransitioner>()        .To<ViewFullscreenTransitioner>()        .AsSingle();
            Container.Bind<IViewLevelStageController>()          .To<ViewLevelStageController>()          .AsSingle();
            Container.Bind<IViewCameraEffectsCustomAnimator>()   .To<ViewCameraEffectsCustomAnimator>()   .AsSingle();
            Container.Bind<IRotatingPossibilityIndicator>()      .To<RotatingPossibilityIndicator>()      .AsTransient();
            
            Container.Bind<IViewMazeAdditionalBackgroundGeometryInitializer>()
                .To<ViewMazeAdditionalBackgroundGeometryInitializerSimple>()
                .AsSingle();
            Container.Bind<IViewMazeAdditionalBackgroundDrawer>()    
                .To<ViewMazeAdditionalBackgroundDrawerRmazor>()  
                .AsSingle();
            
            var inputControllerType = Application.isEditor ? 
                typeof(ViewCommandsProceederInEditor) : typeof(ViewInputCommandsProceeder);
            Container.Bind<IViewInputCommandsProceeder>().To(inputControllerType) .AsSingle();

            Container.Bind<IColorProvider>()       .FromComponentInNewPrefab(colorProvider).AsSingle();
        }

        private void BindMazeItemBlocksAndGroups()
        {
            switch (CommonData.GameId)
            {
                case GameIds.RMAZOR:
                    Container.Bind<IViewMazeItemPath>()    .To<ViewMazeItemPathFilledWithAdditionalBorders>().AsSingle();
                    break;
                case GameIds.ZMAZOR:
                    Container.Bind<IViewMazeItemPath>()    .To<ViewMazeItemPathWithAdditionalBorders>().AsSingle();
                    break;
            }
            Container.Bind<IViewMazeMoneyItem>()             .To<ViewMazeMoneyItemDisc>()            .AsSingle();
            Container.Bind<IViewMazeItemGravityBlock>()      .To<ViewMazeItemGravityBlock>()         .AsSingle();
            Container.Bind<IViewMazeItemMovingTrap>()        .To<ViewMazeItemMovingTrap>()           .AsSingle();
            Container.Bind<IViewMazeItemShredingerBlock>()   .To<ViewMazeItemShredingerBlock>()      .AsSingle();
            Container.Bind<IViewMazeItemTurret>()            .To<ViewMazeItemTurret>()               .AsSingle();
            Container.Bind<IViewMazeItemSpringboard>()       .To<ViewMazeItemSpringboard>()          .AsSingle();
            Container.Bind<IViewMazeItemPortal>()            .To<ViewMazeItemPortal>()               .AsSingle();
            Container.Bind<IViewMazeItemGravityTrap>()       .To<ViewMazeItemGravityTrap>()          .AsSingle();
            Container.Bind<IViewMazeItemTrapReact>()         .To<ViewMazeItemTrapReactSpikes>()      .AsSingle();
            Container.Bind<IViewMazeItemTrapIncreasing>()    .To<ViewMazeItemTrapIncreasing>()       .AsSingle();
            Container.Bind<IViewMazeItemGravityBlockFree>()  .To<ViewMazeItemGravityBlockFree>()     .AsSingle();
            Container.Bind<IViewMazeItemHammer>()            .To<ViewMazeItemHammer>()               .AsSingle();
            Container.Bind<IViewMazeItemSpear>()             .To<ViewMazeItemSpear>()                .AsSingle();
            Container.Bind<IViewMazeItemDiode>()             .To<ViewMazeItemDiode>()                .AsSingle();
            
            Container.Bind<IViewMazePathItemsGroup>()        .To<ViewMazePathItemsGroup>()           .AsSingle();
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
            
            Container.Bind<IViewMazeItemsGroupSet>()         .To<ViewMazeItemsGroupSet>()            .AsSingle();
            
            Container.Bind<IViewTurretProjectile>()          .To<ViewTurretProjectileShuriken>()     .AsSingle();
            Container.Bind<IViewTurretProjectileTail>()      .To<ViewTurretProjectileTail>()         .AsSingle();
            Container.Bind<IViewTurretBody>()                .To<ViewTurretBodySquare>()             .AsSingle();
        }

        private void BindCharacter()
        {
            Container.Bind<IViewCharacter>()         .To<ViewCharacter>()                 .AsSingle();
            Container.Bind<IViewCharacterHead>()     .To<ViewCharacterHead>()             .AsSingle();
            Container.Bind<IViewCharacterEffector>() .To<ViewCharacterEffectorParticles>().AsSingle();
            Container.Bind<IViewCharacterTail>()     .To<ViewCharacterTailDefault>()      .AsSingle();
            Container.Bind<IViewParticleBubble>()    .To<ViewParticleBubble>()            .AsSingle();
            Container.Bind<IViewParticlesThrower>()  .To<ViewParticlesThrower>()          .AsTransient();
        }

        private void BindUiCommon()
        {
            if (!CommonData.Release)
            {
                Container.Bind<IViewUI>()                .To<ViewUIProt>()             .AsSingle();
                Container.Bind<IViewUIGameControls>()    .To<ViewUIGameControlsProt>() .AsSingle();
                Container.Bind<IViewUIGameLogo>()        .To<ViewUIGameLogoFake>()     .AsSingle();
                Container.Bind<IViewUILevelSkipper>()    .To<ViewUILevelSkipperFake>().AsSingle();
            }
            else
            {
                Container.Bind<IViewUI>()                .To<ViewUI>()                 .AsSingle();
                Container.Bind<IViewUIGameControls>()    .To<ViewUIGameControls>()     .AsSingle();
                Container.Bind<IViewUIPrompt>()          .To<ViewUIPrompt>()           .AsSingle();
                Container.Bind<IViewUICongratsMessage>() .To<ViewUICongratsMessage>()  .AsSingle();
                Container.Bind<IViewUIGameLogo>()        .To<ViewUIGameLogoMazeBlade>().AsSingle();
                Container.Bind<IViewUILevelsPanel>()     .To<ViewUILevelsPanel>()      .AsSingle();
                Container.Bind<IViewUIRotationControls>().To<ViewUIRotationControlsButtons>() .AsSingle();
                Container.Bind<IViewUITopButtons>()      .To<ViewUITopButtons>()       .AsSingle();
                Container.Bind<IViewUITutorial>()        .To<ViewUITutorial>()         .AsSingle();
                Container.Bind<IViewUILevelSkipper>()    .To<ViewUILevelSkipperButton>().AsSingle();
            }
        }

        private void BindUiPanelsAndViewers()
        {
            if (!CommonData.Release)
            {
                Container.Bind<IProposalDialogViewer>()      .To<ProposalDialogViewerFake>().AsSingle();
                Container.Bind<IBigDialogViewer>()           .To<BigDialogViewerFake>()     .AsSingle();
                Container.Bind<IDialogPanelsSet>()           .To<DialogPanelsSetFake>()     .AsSingle();
                Container.Bind<IRateGameDialogPanel>()       .To<RateGameDialogPanelFake>() .AsSingle();
            }
            else
            {
                Container.Bind<IBigDialogViewer>()           .To<BigDialogViewer>()         .AsSingle();
                Container.Bind<IProposalDialogViewer>()      .To<ProposalDialogViewer>()    .AsSingle();
                Container.Bind<IDialogPanelsSet>()           .To<DialogPanelsSet>()         .AsSingle();
                
                Container.Bind<IRateGameDialogPanel>()       .To<RateGameDialogPanel>()     .AsSingle();
                Container.Bind<ITutorialDialogPanel>()       .To<TutorialDialogPanel>()     .AsSingle();
                Container.Bind<ISettingSelectorDialogPanel>().To<SettingsSelectorPanel>()   .AsSingle();
                Container.Bind<IShopDialogPanel>()           .To<ShopPanel>()               .AsSingle();
                Container.Bind<IShopMoneyDialogPanel>()      .To<ShopMoneyPanel>()          .AsSingle();
                Container.Bind<IShopHeadsDialogPanel>()      .To<ShopHeadsPanel>()          .AsSingle();
                Container.Bind<IShopTailsDialogPanel>()      .To<ShopTailsPanel>()          .AsSingle();
                Container.Bind<ISettingDialogPanel>()        .To<SettingsDialogPanel>()     .AsSingle();
                Container.Bind<ICharacterDiedDialogPanel>()  .To<CharacterDiedDialogPanel>().AsSingle();
            }
        }

        private void BindTextureProviders()
        {
            Container.Bind<IViewMazeGameLogoTextureProvider>()    .To<ViewMazeGameLogoTextureProviderCirclesToSquares>().AsSingle();
            Container.Bind<IViewMazeBackgroundTextureController>().To<ViewMazeBackgroundTextureController>()        .AsSingle();
            
            Container.Bind<IFullscreenTextureProviderTriangles2>().To<FullscreenTriangles2TextureProvider>()        .AsSingle();
            Container.Bind<IFullscreenTransitionTextureProviderTriaHex>().To<FullscreenTransitionTextureProvideTriaHex>().AsSingle();
            Container.Bind<IFullscreenTransitionTextureProviderCircles>().To<FullscreenTransitionTextureProviderCircles>() .AsSingle();
            
            Container.Bind<IFullscreenTextureProviderMetaBalls1>().To<FullscreenTextureProviderMetaBalls1>() .AsSingle();
            Container.Bind<IFullscreenTextureProviderMetaBalls2>().To<FullscreenTextureProviderMetaBalls2>() .AsSingle();
            Container.Bind<IFullscreenTextureProviderMetaBalls3>().To<FullscreenTextureProviderMetaBalls3>() .AsSingle();
            Container.Bind<IFullscreenTextureProviderMetaBalls4>().To<FullscreenTextureProviderMetaBalls4>() .AsSingle();
            Container.Bind<IFullscreenTextureProviderMetaBalls5>().To<FullscreenTextureProviderMetaBalls5>() .AsSingle();
            
            Container.Bind<IFullscreenTransitionTextureProvider>().To<FullscreenTransitionTextureProviderCirclesToSquares>().AsSingle();
        }

        private void BindOther()
        {
            Container.Bind<IGameController>().To<GameControllerMVC>().AsSingle();
            Container.Bind<IDebugManager>()  .To<DebugManager>()  .AsSingle();
            Container.Bind<IManagersGetter>().To<ManagersGetter>().AsSingle();
            Container.Bind(typeof(IAudioManagerRmazor), typeof(IAudioManager))
                .To<AudioManagerRmazor>()
                .AsSingle();
            Container.Bind<IViewBetweenLevelAdLoader>().To<ViewBetweenLevelAdLoader>().AsSingle();
        }
    }
}

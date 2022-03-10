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
using RMAZOR.Views.Common.CongratulationItems;
using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using RMAZOR.Views.UI.StartLogo;
using UnityEngine;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstallerImplBase
    {
        public GameObject colorProvider;

        public override void InstallBindings()
        {
            base.InstallBindings();
            Container.Bind<IGameController>()                .To<GameController>()                .AsSingle();
            
            #region model

            Container.Bind<IInputScheduler>()                .To<InputScheduler>()                .AsSingle();
            Container.Bind<IInputSchedulerGameProceeder>()   .To<InputSchedulerGameProceeder>()   .AsSingle();
            Container.Bind<IInputSchedulerUiProceeder>()     .To<InputSchedulerUiProceeder>()     .AsSingle();
            Container.Bind<IModelData>()                     .To<ModelData>()                     .AsSingle();
            Container.Bind<IModelMazeRotation>()             .To<ModelMazeRotation>()             .AsSingle();
            Container.Bind<IModelCharacter>()                .To<ModelCharacter>()                .AsSingle();
            Container.Bind<IModelGame>()                     .To<ModelGame>()                     .AsSingle();
            Container.Bind<IModelLevelStaging>()             .To<ModelLevelStaging>()             .AsSingle();
            Container.Bind<IPathItemsProceeder>()            .To<PathItemsProceeder>()            .AsSingle();
            Container.Bind<ITrapsMovingProceeder>()          .To<TrapsMovingProceeder>()          .AsSingle();
            Container.Bind<IGravityItemsProceeder>()         .To<GravityItemsProceeder>()         .AsSingle();
            Container.Bind<ITrapsReactProceeder>()           .To<TrapsReactProceeder>()           .AsSingle();
            Container.Bind<ITurretsProceeder>()              .To<TurretsProceeder>()              .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()      .To<TrapsIncreasingProceeder>()      .AsSingle();
            Container.Bind<IPortalsProceeder>()              .To<PortalsProceeder>()              .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()     .To<ShredingerBlocksProceeder>()     .AsSingle();
            Container.Bind<ISpringboardProceeder>()          .To<SpringboardProceeder>()          .AsSingle();

            #endregion

            #region view
            
            Container.Bind<IMazeShaker>()                    .To<MazeShaker>()                    .AsSingle();
            Container.Bind<IMazeCoordinateConverter>()       .To<MazeCoordinateConverter>()       .AsSingle();
            Container.Bind<IContainersGetter>()              .To<ContainersGetterRmazor>()        .AsSingle();
            Container.Bind<IMazeItemsCreator>()              .To<MazeItemsCreator>()              .AsSingle();

            if (!CommonData.Release)
            {
                Container.Bind<IViewUI>()                    .To<ViewUIProt>()                    .AsSingle();
                Container.Bind<IViewUIGameControls>()        .To<ViewUIGameControlsProt>()        .AsSingle();
            }
            else
            {
                Container.Bind<IViewUI>()                    .To<ViewUI>()                        .AsSingle();
                Container.Bind<IViewUIGameControls>()        .To<ViewUIGameControls>()            .AsSingle();
                Container.Bind<IViewUIPrompt>()              .To<ViewUIPrompt>()                  .AsSingle();
                Container.Bind<IViewUICongratsMessage>()     .To<ViewUICongratsMessage>()         .AsSingle();
                Container.Bind<IViewUIStartLogo>()           .To<ViewUIStartLogoMazeBlade>()      .AsSingle();
                Container.Bind<IViewUILevelsPanel>()         .To<ViewUILevelsPanel>()             .AsSingle();
                Container.Bind<IViewUIRotationControls>()    .To<ViewUIRotationControls>()        .AsSingle();
                Container.Bind<IViewUITopButtons>()          .To<ViewUITopButtons>()              .AsSingle();
                Container.Bind<IViewUITutorial>()            .To<ViewUITutorial>()                .AsSingle();
            }
            
            Container.Bind<IViewGame>()                      .To<ViewGame>()                      .AsSingle();
            Container.Bind<IViewMazeCommon>()                .To<ViewMazeCommon>()                .AsSingle();

            Container.Bind<IViewMazeRotation>()              .To<ViewMazeRotation>()              .AsSingle();
            Container.Bind<IViewBackground>()            .To<ViewBackground>()            .AsSingle();
            Container.Bind<IViewMazeBackgroundIdleItems>()   .To<ViewMazeBackgroundIdleItems>()   .AsSingle();
            Container.Bind<IViewMazeBackgroundCongradItems>().To<ViewMazeBackgroundCongradItems2>().AsSingle();
            Container.Bind<IViewMazeForeground>()            .To<ViewMazeForeground>()            .AsSingle();
            Container.Bind<IViewBetweenLevelTransitioner>()  .To<ViewBetweenLevelTransitioner>()  .AsSingle();
            Container.Bind<IViewLevelStageController>()      .To<ViewLevelStageController>()      .AsSingle();
            
            Container.Bind<IViewCharacter>()                 .To<ViewCharacter>()                 .AsSingle();
            Container.Bind<IViewCharacterHead>()             .To<ViewCharacterHead>()             .AsSingle();
            Container.Bind<IViewCharacterEffector>()         .To<ViewCharacterEffectorParticles>().AsSingle();
            Container.Bind<IViewCharacterTail>()             .To<ViewCharacterTailSimple>()       .AsSingle();
            
            Container.Bind<IViewTurretProjectileTail>()      .To<ViewTurretProjectileTailFake>()  .AsSingle();

            Container.Bind<IViewMazeMoneyItem>()             .To<ViewMazeMoneyItemHexagon>()      .AsSingle();
            Container.Bind<IViewMazeItemPath>()              .To<ViewMazeItemPathFilled>()        .AsSingle();
            Container.Bind<IViewMazeItemGravityBlock>()      .To<ViewMazeItemGravityBlock>()      .AsSingle();
            Container.Bind<IViewMazeItemMovingTrap>()        .To<ViewMazeItemMovingTrap>()        .AsSingle();
            Container.Bind<IViewMazeItemShredingerBlock>()   .To<ViewMazeItemShredingerBlock>()   .AsSingle();
            Container.Bind<IViewMazeItemTurret>()            .To<ViewMazeItemTurret>()            .AsSingle();
            Container.Bind<IViewMazeItemSpringboard>()       .To<ViewMazeItemSpringboard>()       .AsSingle();
            Container.Bind<IViewMazeItemPortal>()            .To<ViewMazeItemPortal>()            .AsSingle();
            Container.Bind<IViewMazeItemGravityTrap>()       .To<ViewMazeItemGravityTrap>()       .AsSingle();
            Container.Bind<IViewMazeItemTrapReact>()         .To<ViewMazeItemTrapReactSpikes>()   .AsSingle();
            Container.Bind<IViewMazeItemTrapIncreasing>()    .To<ViewMazeItemTrapIncreasing>()    .AsSingle();
            Container.Bind<IViewMazeItemGravityBlockFree>()  .To<ViewMazeItemGravityBlockFree>()  .AsSingle();
            
            Container.Bind<IViewMazePathItemsGroup>()        .To<ViewMazePathItemsGroup>()        .AsSingle();
            Container.Bind<IViewMazeMovingItemsGroup>()      .To<ViewMazeMovingItemsGroup>()      .AsSingle();
            Container.Bind<IViewMazeShredingerBlocksGroup>() .To<ViewMazeShredingerBlocksGroup>() .AsSingle();
            Container.Bind<IViewMazeTurretsGroup>()          .To<ViewMazeTurretsGroup>()          .AsSingle();
            Container.Bind<IViewMazeSpringboardItemsGroup>() .To<ViewMazeSpringboardItemsGroup>() .AsSingle();
            Container.Bind<IViewMazePortalsGroup>()          .To<ViewMazePortalsGroup>()          .AsSingle();
            Container.Bind<IViewMazeTrapsReactItemsGroup>()  .To<ViewMazeTrapsReactSpikesItemGroup>()   .AsSingle();
            Container.Bind<IViewMazeTrapsIncItemsGroup>()    .To<ViewMazeTrapsIncItemsGroup>()    .AsSingle();
            Container.Bind<IViewMazeGravityItemsGroup>()     .To<ViewMazeGravityItemsGroup>()     .AsSingle();

            Container.Bind<IViewMazeItemsGroupSet>()         .To<ViewMazeItemsGroupSet>()         .AsSingle();
            Container.Bind<IViewMazeAdditionalBackground>()  .To<ViewMazeAdditionalBackground>()  .AsSingle();
            
            Container.Bind<IViewMazeAdditionalBackgroundGeometryInitializer>().To<ViewMazeAdditionalBackgroundGeometryInitializerSimple>().AsSingle();
            Container.Bind<IViewMazeAdditionalBackgroundDrawer>()             .To<ViewMazeAdditionalBackgroundDrawerSimple>()       .AsSingle();
            Container.Bind<IViewMazeBackgroundTextureController>()            .To<ViewMazeBackgroundTextureController>()            .AsSingle();
            Container.Bind<IViewMazeBackgroundLinesTextureProvider>()         .To<ViewMazeBackgroundLinesTextureProvider>()         .AsSingle();
            Container.Bind<IViewMazeBackgroundCirclesTextureProvider>()       .To<ViewMazeBackgroundCirclesTextureProvider>()       .AsSingle();
            Container.Bind<IViewMazeBackgroundCircles2TextureProvider>()      .To<ViewMazeBackgroundCircles2TextureProvider>()      .AsSingle();
            Container.Bind<IViewMazeBackgroundTrianglesTextureProvider>()     .To<ViewMazeBackgroundTrianglesTextureProvider>()     .AsSingle();

            if (!CommonData.Release)
            {
                Container.Bind<IProposalDialogViewer>()      .To<ProposalDialogViewerFake>()      .AsSingle();
                Container.Bind<IBigDialogViewer>()           .To<BigDialogViewerFake>()           .AsSingle();
                Container.Bind<IDialogPanels>()              .To<DialogPanelsFake>()              .AsSingle();
            }
            else
            {
                Container.Bind<IBigDialogViewer>()           .To<BigDialogViewer>()               .AsSingle();
                Container.Bind<IProposalDialogViewer>()      .To<ProposalDialogViewer>()          .AsSingle();
                Container.Bind<IDialogPanels>()              .To<DialogPanels>()                  .AsSingle();
                Container.Bind<ISettingSelectorDialogPanel>().To<SettingsSelectorPanel>()         .AsSingle();
                Container.Bind<IShopDialogPanel>()           .To<ShopPanel>()                     .AsSingle();
                Container.Bind<IShopMoneyDialogPanel>()      .To<ShopMoneyPanel>()                .AsSingle();
                Container.Bind<IShopHeadsDialogPanel>()      .To<ShopHeadsPanel>()                .AsSingle();
                Container.Bind<IShopTailsDialogPanel>()      .To<ShopTailsPanel>()                .AsSingle();
                Container.Bind<ISettingDialogPanel>()        .To<SettingsDialogPanel>()           .AsSingle();
#if UNITY_ANDROID
                Container.Bind<ICharacterDiedDialogPanel>()  .To<CharacterDiedDialogPanel2>()     .AsSingle();
#elif UNITY_IOS
                Container.Bind<ICharacterDiedDialogPanel>()  .To<CharacterDiedDialogPanel>()     .AsSingle();
#endif
                Container.Bind<IRateGameDialogPanel>()       .To<RateGameDialogPanel>()           .AsSingle();
            }
            Container.Bind<IViewInputCommandsProceeder>()    .To<ViewInputCommandsProceeder>()    .AsSingle();
            Container.Bind<IViewInputTouchProceeder>()       .To<ViewInputTouchProceeder>()       .AsSingle();
            Container.Bind<IRotatingPossibilityIndicator>()  .To<RotatingPossibilityIndicator>()  .AsTransient();
#if UNITY_EDITOR
            Container.Bind<IViewInputController>()           .To<ViewInputControllerInEditor>()   .AsSingle();
#else
            Container.Bind<IViewInputController>()           .To<ViewInputController>()           .AsSingle();
#endif
            
            #endregion

            #region other

            Container.Bind<IColorProvider>()     .FromComponentInNewPrefab(colorProvider) .AsSingle();
            Container.Bind<IDebugManager>()      .To<DebugManager>()                      .AsSingle();
            Container.Bind<IManagersGetter>()    .To<ManagersGetter>()                    .AsSingle();
            Container.Bind(typeof(IAudioManagerRmazor), typeof(IAudioManager))
                .To<AudioManagerRmazor>()
                .AsSingle();

            #endregion
        }
    }
}

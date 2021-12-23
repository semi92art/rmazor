using Controllers;
using DialogViewers;
using Entities;
using Games.RazorMaze;
using Games.RazorMaze.Controllers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.InputSchedulers;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.MazeItems.Additional;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using UI.Panels;
using UI.Panels.ShopPanels;
using UnityEngine;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstallerImplBase
    {
        public static bool Release;
        
        public ModelSettings modelSettings;
        public ViewSettings  viewSettings;
        public GameObject    cameraProvider;
        public GameObject    colorProvider;

        public override void InstallBindings()
        {
            base.InstallBindings();
            Container.Bind<IGameController>()                .To<GameController>()                .AsSingle();
            
            #region model
            
            Container.Bind<ModelSettings>()                  .FromScriptableObject(modelSettings) .AsSingle();
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
            
            Container.Bind<ViewSettings>()                   .FromScriptableObject(viewSettings)  .AsSingle();
            Container.Bind<IMazeShaker>()                    .To<MazeShaker>()                    .AsSingle();
            Container.Bind<IMazeCoordinateConverter>()       .To<MazeCoordinateConverter>()       .AsSingle();
            Container.Bind<IContainersGetter>()              .To<ContainersGetter>()              .AsSingle();
            Container.Bind<IMazeItemsCreator>()              .To<MazeItemsCreator>()              .AsSingle();

            if (!Release)
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
                Container.Bind<IViewUIStartLogo>()           .To<ViewUIStartLogo>()               .AsSingle();
                Container.Bind<IViewUILevelsPanel>()         .To<ViewUILevelsPanel>()             .AsSingle();
                Container.Bind<IViewUIRotationControls>()    .To<ViewUIRotationControls>()        .AsSingle();
                Container.Bind<IViewUITopButtons>()          .To<ViewUITopButtons>()              .AsSingle();
                Container.Bind<IViewUITutorial>()           .To<ViewUITutorial>()               .AsSingle();
            }
            
            Container.Bind<IViewGame>()                      .To<ViewGame>()                      .AsSingle();
            Container.Bind<IViewMazeCommon>()                .To<ViewMazeCommon>()                .AsSingle();

            Container.Bind<IViewMazeRotation>()              .To<ViewMazeRotation>()              .AsSingle();
            Container.Bind<IViewMazeBackground>()            .To<ViewMazeBackground>()            .AsSingle();
            Container.Bind<IViewAppearTransitioner>()        .To<ViewAppearTransitioner>()        .AsSingle();
            Container.Bind<IViewLevelStageController>()      .To<ViewLevelStageController>()      .AsSingle();
            
            Container.Bind<IViewCharacter>()                 .To<ViewCharacter>()                 .AsSingle();
            Container.Bind<IViewCharacterEffector>()         .To<ViewCharacterEffectorParticles>().AsSingle();
            Container.Bind<IViewCharacterTail>()             .To<ViewCharacterTailSimple>()       .AsSingle();
            
            Container.Bind<IViewTurretBulletTail>()          .To<ViewTurretBulletTailFake>()      .AsSingle();

            Container.Bind<IViewMazeItemPath>()              .To<ViewMazeItemPath>()              .AsSingle();
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
            Container.Bind<IViewMazeTrapsReactItemsGroup>()  .To<ViewMazeTrapsReactItemGroup>()   .AsSingle();
            Container.Bind<IViewMazeTrapsIncItemsGroup>()    .To<ViewMazeTrapsIncItemsGroup>()    .AsSingle();
            Container.Bind<IViewMazeGravityItemsGroup>()     .To<ViewMazeGravityItemsGroup>()     .AsSingle();

            if (!Release)
            {
                Container.Bind<IProposalDialogViewer>()      .To<ProposalDialogViewerFake>()      .AsSingle();
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
                Container.Bind<IWofDialogPanel>()            .To<WofDialogPanel>()                .AsSingle();
                Container.Bind<IWofRewardPanel>()            .To<WofRewardDialogPanel>()          .AsSingle();
                Container.Bind<ISettingDialogPanel>()        .To<SettingsDialogPanel>()           .AsSingle();
                Container.Bind<ICharacterDiedDialogPanel>()  .To<CharacterDiedDialogPanel>()      .AsSingle();
                Container.Bind<IRateGameDialogPanel>()       .To<RateGameDialogPanel>()           .AsSingle();
            }
            
            Container.Bind<IViewInputCommandsProceeder>()    .To<ViewInputCommandsProceeder>()    .AsSingle();
            Container.Bind<IViewInputTouchProceeder>()       .To<ViewInputTouchProceeder>()       .AsSingle();
#if UNITY_EDITOR
            Container.Bind<IViewInputController>()           .To<ViewInputControllerInEditor>()   .AsSingle();
#else
            Container.Bind<IViewInputController>()           .To<ViewInputController>()           .AsSingle();
#endif
            
            #endregion

            #region other

            Container.Bind<ICameraProvider>()   .FromComponentInNewPrefab(cameraProvider).AsSingle();
            Container.Bind<IColorProvider>()    .FromComponentInNewPrefab(colorProvider) .AsSingle();
            Container.Bind<ILoadingController>().To<LoadingController>()                 .AsSingle().When(_ => Release);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Container.Bind<IDebugManager>()     .To<DebugManager>()                      .AsSingle();
#endif
            Container.Bind<IAudioManager>()     .To<AudioManager>()                      .AsSingle();
            Container.Bind<IManagersGetter>()   .To<ManagersGetter>()                    .AsSingle();

            #endregion
        }
    }
}

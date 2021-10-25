using Controllers;
using DialogViewers;
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
using Settings;
using UI.Panels;
using UnityEngine;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstallerImplBase
    {
        public static bool Release;
        public ModelSettings modelSettings;
        public ViewSettings viewSettings;
        public GameObject cameraProvider;

        public override void InstallBindings()
        {
            base.InstallBindings();
            
            Container.Bind<IGameController>()               .To<RazorMazeGameController>()       .AsSingle();
            
            #region model
            
            Container.Bind<ModelSettings>()                 .FromScriptableObject(modelSettings) .AsSingle();
            Container.Bind<IInputScheduler>()               .To<InputScheduler>()                .AsSingle();
            Container.Bind<IInputSchedulerGameProceeder>()  .To<InputSchedulerGameProceeder>()   .AsSingle();
            Container.Bind<IInputSchedulerUiProceeder>()    .To<InputSchedulerUiProceeder>()     .AsSingle();
            Container.Bind<IModelData>()                    .To<ModelData>()                     .AsSingle();
            Container.Bind<IModelMazeRotation>()            .To<ModelMazeRotation>()             .AsSingle();
            Container.Bind<IModelCharacter>()               .To<ModelCharacter>()                .AsSingle();
            Container.Bind<IModelGame>()                    .To<ModelGame>()                     .AsSingle();
            Container.Bind<IModelLevelStaging>()            .To<ModelLevelStaging>()             .AsSingle();
            Container.Bind<IPathItemsProceeder>()           .To<PathItemsProceeder>()            .AsSingle();
            Container.Bind<ITrapsMovingProceeder>()         .To<TrapsMovingProceeder>()          .AsSingle();
            Container.Bind<IGravityItemsProceeder>()        .To<GravityItemsProceeder>()         .AsSingle();
            Container.Bind<ITrapsReactProceeder>()          .To<TrapsReactProceeder>()           .AsSingle();
            Container.Bind<ITurretsProceeder>()             .To<TurretsProceeder>()              .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()     .To<TrapsIncreasingProceeder>()      .AsSingle();
            Container.Bind<IPortalsProceeder>()             .To<PortalsProceeder>()              .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()    .To<ShredingerBlocksProceeder>()     .AsSingle();
            Container.Bind<ISpringboardProceeder>()         .To<SpringboardProceeder>()          .AsSingle();

            #endregion

            #region view
            
            Container.Bind<ViewSettings>()                  .FromScriptableObject(viewSettings)  .AsSingle();

            Container.Bind<IMazeShaker>()                   .To<MazeShaker>()                    .AsSingle();
            Container.Bind<IMazeCoordinateConverter>()      .To<MazeCoordinateConverter>()       .AsSingle();
            Container.Bind<IContainersGetter>()             .To<ContainersGetter>()              .AsSingle();
            Container.Bind<IMazeItemsCreator>()             .To<MazeItemsCreator>()              .AsSingle();
            
            Container.Bind<IViewGame>()                     .To<ViewGame>()                      .AsSingle();
            Container.Bind<IViewMazeCommon>()               .To<ViewMazeCommon>()                .AsSingle();
            Container.Bind<IViewUI>()                       .To<ViewUI>()                        .AsSingle().When(_ => Release);
            Container.Bind<IViewUI>()                       .To<ViewUIProt>()                    .AsSingle().When(_ => !Release);
            Container.Bind<IViewUIGameControls>()           .To<ViewUIGameControls>()            .AsSingle().When(_ => Release);
            Container.Bind<IViewUIGameControls>()           .To<ViewUIGameControlsProt>()        .AsSingle().When(_ => !Release);
            Container.Bind<IViewMazeRotation>()             .To<ViewMazeRotation>()              .AsSingle();
            Container.Bind<IViewMazeBackground>()           .To<ViewMazeBackground>()            .AsSingle();
            Container.Bind<IViewAppearTransitioner>()       .To<ViewAppearTransitioner>()        .AsSingle();
            Container.Bind<IViewLevelStageController>()     .To<ViewLevelStageController>()      .AsSingle();
            
            Container.Bind<IViewCharacter>()                .To<ViewCharacter>()                 .AsSingle();
            Container.Bind<IViewCharacterEffector>()        .To<ViewCharacterEffectorParticles>().AsSingle();
            Container.Bind<IViewCharacterTail>()            .To<ViewCharacterTailSimple>()       .AsSingle();
            
            Container.Bind<IViewTurretBulletTail>()         .To<ViewTurretBulletTailSimple>()    .AsSingle();

            Container.Bind<IViewMazeItemPath>()             .To<ViewMazeItemPath>()              .AsSingle();
            Container.Bind<IViewMazeItemGravityBlock>()     .To<ViewMazeItemGravityBlock>()      .AsSingle();
            Container.Bind<IViewMazeItemMovingTrap>()       .To<ViewMazeItemMovingTrap>()        .AsSingle();
            Container.Bind<IViewMazeItemShredingerBlock>()  .To<ViewMazeItemShredingerBlock>()   .AsSingle();
            Container.Bind<IViewMazeItemTurret>()           .To<ViewMazeItemTurret>()            .AsSingle();
            Container.Bind<IViewMazeItemSpringboard>()      .To<ViewMazeItemSpringboard>()       .AsSingle();
            Container.Bind<IViewMazeItemPortal>()           .To<ViewMazeItemPortal>()            .AsSingle();
            Container.Bind<IViewMazeItemGravityTrap>()      .To<ViewMazeItemGravityTrap>()       .AsSingle();
            Container.Bind<IViewMazeItemTrapReact>()        .To<ViewMazeItemTrapReactSpikes>()   .AsSingle();
            Container.Bind<IViewMazeItemTrapIncreasing>()   .To<ViewMazeItemTrapIncreasing>()    .AsSingle();
            Container.Bind<IViewMazeItemGravityBlockFree>() .To<ViewMazeItemGravityBlockFree>()  .AsSingle();
            
            Container.Bind<IViewMazePathItemsGroup>()       .To<ViewMazePathItemsGroup>()        .AsSingle();
            Container.Bind<IViewMazeMovingItemsGroup>()     .To<ViewMazeMovingItemsGroup>()      .AsSingle();
            Container.Bind<IViewMazeShredingerBlocksGroup>().To<ViewMazeShredingerBlocksGroup>() .AsSingle();
            Container.Bind<IViewMazeTurretsGroup>()         .To<ViewMazeTurretsGroup>()          .AsSingle();
            Container.Bind<IViewMazeSpringboardItemsGroup>().To<ViewMazeSpringboardItemsGroup>() .AsSingle();
            Container.Bind<IViewMazePortalsGroup>()         .To<ViewMazePortalsGroup>()          .AsSingle();
            Container.Bind<IViewMazeTrapsReactItemsGroup>() .To<ViewMazeTrapsReactItemGroup>()   .AsSingle();
            Container.Bind<IViewMazeTrapsIncItemsGroup>()   .To<ViewMazeTrapsIncItemsGroup>()    .AsSingle();
            Container.Bind<IViewMazeGravityItemsGroup>()    .To<ViewMazeGravityItemsGroup>()     .AsSingle();

            Container.Bind<IDialogPanels>()                 .To<DialogPanels>()                  .AsSingle().When(_ => Release);
            Container.Bind<ILoadingDialogPanel>()           .To<LoadingPanel>()                  .AsSingle().When(_ => Release);
            Container.Bind<ISettingSelectorDialogPanel>()   .To<SettingsSelectorPanel>()         .AsSingle().When(_ => Release);
            Container.Bind<IShopDialogPanel>()              .To<ShopPanel>()                     .AsSingle().When(_ => Release);
            Container.Bind<IWheelOfFortuneDialogPanel>()    .To<WheelOfFortunePanel>()           .AsSingle().When(_ => Release);
            Container.Bind<IWheelOfFortuneRewardPanel>()    .To<WheelOfFortuneRewardPanel>()     .AsSingle().When(_ => Release);
            Container.Bind<IDialogViewer>()                 .To<FullScreenDialogViewer>()        .AsSingle().When(_ => Release);
            Container.Bind<INotificationViewer>()           .To<NotificationViewer>()            .AsSingle().When(_ => Release);
            Container.Bind<ITransitionRenderer>()           .To<CircleTransitionRenderer>()      .AsSingle().When(_ => Release);
            Container.Bind<ILoadingController>()            .To<LoadingController>()             .AsSingle().When(_ => Release);
            Container.Bind<IDailyBonusDialogPanel>()        .To<DailyBonusPanel>()               .AsSingle().When(_ => Release);
            Container.Bind<ISettingDialogPanel>()           .To<SettingsPanel>()                 .AsSingle().When(_ => Release);
            Container.Bind<IViewUIPrompts>()                .To<ViewUIPrompts>()                 .AsSingle().When(_ => Release);

#if UNITY_EDITOR
            Container.Bind<IViewInputConfigurator>()        .To<ViewInputConfiguratorInEditor>() .AsSingle();
#else
            Container.Bind<IViewInputConfigurator>()        .To<ViewInputConfigurator>()         .AsSingle();
#endif
            
            #endregion
            
            #region settings

            Container.Bind<ISoundSetting>()       .To<SoundSetting>()        .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()        .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>().AsSingle();
            Container.Bind<IVibrationSetting>()   .To<VibrationSetting>()    .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()     .AsSingle();
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()        .AsSingle();
            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()      .AsSingle();

            #endregion

            #region other

            Container.Bind<ICameraProvider>().FromComponentInNewPrefab(cameraProvider).AsSingle();

            #endregion
        }
    }
}
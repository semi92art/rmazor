using Games.RazorMaze;
using Games.RazorMaze.Controllers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.MazeItems.Additional;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using UnityEngine;
using Utils;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstallerImplBase
    {
        public ModelSettings modelSettings;
        public ViewSettings viewSettings;
        public Object gameTimeProvider;
        public Object uiTimeProvider;
        
        public override void InstallBindings()
        {
            base.InstallBindings();
            
            bool release = GameClientUtils.GameMode == (int) EGameMode.Release;

            #region model
            
            Container.Bind<ModelSettings>()                     .FromScriptableObject(modelSettings)        .AsSingle();
            Container.Bind<ViewSettings>()                      .FromScriptableObject(viewSettings)         .AsSingle();
            Container.Bind<IViewGame>()                         .To<ViewGame>()                             .AsSingle();
            Container.Bind<IGameController>()                   .To<RazorMazeGameController>()              .AsSingle();
            Container.Bind<IModelData>()                    .To<ModelData>()                        .AsSingle();
            Container.Bind<IModelMazeRotation>()                .To<ModelMazeRotation>()                    .AsSingle();
            Container.Bind<IModelCharacter>()                   .To<ModelCharacter>()                       .AsSingle();
            Container.Bind<IModelGame>()                        .To<ModelGame>()                            .AsSingle();
            Container.Bind<ILevelStagingModel>()                .To<LevelStagingModel>()                    .AsSingle();
            Container.Bind<ICoordinateConverter>()              .To<CoordinateConverter>()                  .AsSingle();
            Container.Bind<IContainersGetter>()                 .To<ContainersGetter>()                     .AsSingle();
            Container.Bind<IPathItemsProceeder>()               .To<PathItemsProceeder>()                   .AsSingle();
            Container.Bind<ITrapsMovingProceeder>()             .To<TrapsMovingProceeder>()                 .AsSingle();
            Container.Bind<IGravityItemsProceeder>()            .To<GravityItemsProceeder>()                .AsSingle();
            Container.Bind<ITrapsReactProceeder>()              .To<TrapsReactProceeder>()                  .AsSingle();
            Container.Bind<ITurretsProceeder>()                 .To<TurretsProceeder>()                     .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()         .To<TrapsIncreasingProceeder>()             .AsSingle();
            Container.Bind<IPortalsProceeder>()                 .To<PortalsProceeder>()                     .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()        .To<ShredingerBlocksProceeder>()            .AsSingle();
            Container.Bind<ISpringboardProceeder>()             .To<SpringboardProceeder>()                 .AsSingle();
            // Container.Bind<IGameTimeProvider>()                 .FromComponentsInNewPrefab(gameTimeProvider).AsSingle();
            // Container.Bind<IUiTimeProvider>()                   .FromComponentsInNewPrefab(uiTimeProvider)  .AsCached();
            
            #endregion

            #region view release
            
            Container.Bind<IMazeItemsCreator>()                 .To<MazeItemsCreator>()                     .AsSingle().When(_ => release);
            Container.Bind<IViewMazeCommon>()                   .To<ViewMazeCommon>()                       .AsSingle().When(_ => release);
            Container.Bind<IViewMazeBackground>()               .To<ViewMazeBackground>()                   .AsSingle().When(_ => release);
            Container.Bind<IViewCharacter>()                    .To<ViewCharacter>()                        .AsSingle().When(_ => release);
            Container.Bind<IViewCharacterEffector>()            .To<ViewCharacterEffectorParticles>()       .AsSingle().When(_ => release);
            Container.Bind<IViewCharacterTail>()                .To<ViewCharacterTailSimple>()              .AsSingle().When(_ => release);
            Container.Bind<ITurretBulletTail>()                 .To<TurretBulletTailSimple>()               .AsSingle().When(_ => release);
            Container.Bind<IViewMazePathItemsGroup>()           .To<ViewMazePathItemsGroup>()               .AsSingle().When(_ => release);
            Container.Bind<IViewMazeMovingItemsGroup>()         .To<ViewMazeMovingItemsGroup>()             .AsSingle().When(_ => release);
            Container.Bind<IViewMazeShredingerBlocksGroup>()    .To<ViewMazeShredingerBlocksGroup>()        .AsSingle().When(_ => release);
            Container.Bind<IViewMazeTurretsGroup>()             .To<ViewMazeTurretsGroup>()                 .AsSingle().When(_ => release);
            Container.Bind<IViewMazeSpringboardItemsGroup>()    .To<ViewMazeSpringboardItemsGroup>()        .AsSingle().When(_ => release);
            Container.Bind<IViewMazePortalsGroup>()             .To<ViewMazePortalsGroup>()                 .AsSingle().When(_ => release);
            Container.Bind<IViewMazeTrapsReactItemsGroup>()     .To<ViewMazeTrapsReactItemGroup>()          .AsSingle().When(_ => release);
            Container.Bind<IViewMazeTrapsIncreasingItemsGroup>().To<ViewMazeTrapsIncreasingItemsGroup>()    .AsSingle().When(_ => release);
            
            Container.Bind<IViewMazeItemPath>()                 .To<ViewMazeItemPath>()                     .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemGravityBlock>()         .To<ViewMazeItemGravityBlock>()             .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemMovingTrap>()           .To<ViewMazeItemMovingTrap>()               .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemShredingerBlock>()      .To<ViewMazeItemShredingerBlock>()          .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemTurret>()               .To<ViewMazeItemTurret>()                   .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemSpringboard>()          .To<ViewMazeItemSpringboard>()              .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemPortal>()               .To<ViewMazeItemPortal>()                   .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemGravityTrap>()          .To<ViewMazeItemGravityTrap>()              .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemTrapReact>()            .To<ViewMazeItemTrapReact>()                .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemTrapIncreasing>()       .To<ViewMazeItemTrapIncreasing>()           .AsSingle().When(_ => release);
            
            Container.Bind<IViewMazeRotation>()                 .To<ViewMazeRotationProt>()                 .AsSingle();
            Container.Bind<IViewUI>()                           .To<ViewUIProt>()                           .AsSingle();
            // Container.Bind<IViewMazeRotation>()              .To<ViewMazeRotation>()                     .AsSingle().When(_ => release);
            // Container.Bind<IViewUI>()                        .To<ViewUI>()                               .AsSingle().When(_ => release);

            #endregion
            
            #region device
            
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            
#endif
            #endregion
            
            
#if UNITY_EDITOR
            Container.Bind<IInputScheduler>()                   .To<InputSchedulerInEditor>()                .AsSingle();
            Container.Bind<IInputConfigurator>()                .To<RazorMazeInputConfiguratorProt>()        .AsSingle();
#else
            Container.Bind<IInputConfigurator>()                .To<RazorMazeInputConfigurator>()            .AsSingle();
            Container.Bind<IInputScheduler>()                   .To<InputSchedulerInGame>()                  .AsSingle();
#endif
        }
    }
}
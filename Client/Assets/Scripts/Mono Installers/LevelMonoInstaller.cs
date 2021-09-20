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
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using TimeProviders;
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
            
            bool prot = GameClientUtils.GameMode == (int) EGameMode.Prototyping;
            bool release = GameClientUtils.GameMode == (int) EGameMode.Release;

            #region model
            
            Container.Bind<ModelSettings>()                     .FromScriptableObject(modelSettings)        .AsSingle();
            Container.Bind<ViewSettings>()                      .FromScriptableObject(viewSettings)         .AsSingle();
            Container.Bind<IViewGame>()                         .To<ViewGame>()                             .AsSingle();
            Container.Bind<IGameController>()                   .To<RazorMazeGameController>()              .AsSingle();
            Container.Bind<IModelMazeData>()                    .To<ModelMazeData>()                        .AsSingle();
            Container.Bind<IModelMazeRotation>()                .To<ModelMazeRotation>()                    .AsSingle();
            Container.Bind<IModelCharacter>()                   .To<ModelCharacter>()                       .AsSingle();
            Container.Bind<IModelGame>()                        .To<ModelGame>()                            .AsSingle();
            Container.Bind<ILevelStagingModel>()                .To<LevelStagingModel>()                    .AsSingle();
            Container.Bind<IInputScheduler>()                   .To<InputScheduler>()                       .AsSingle();
            Container.Bind<ICoordinateConverter>()              .To<CoordinateConverter>()                  .AsSingle();
            Container.Bind<IContainersGetter>()                 .To<ContainersGetter>()                     .AsSingle();
            Container.Bind<IPathItemsProceeder>()               .To<PathItemsProceeder>()                   .AsSingle();
            Container.Bind<IMovingItemsProceeder>()             .To<TrapsMovingProceeder>()                 .AsSingle();
            Container.Bind<IGravityItemsProceeder>()            .To<GravityItemsProceeder>()                .AsSingle();
            Container.Bind<ITrapsReactProceeder>()              .To<TrapsReactProceeder>()                  .AsSingle();
            Container.Bind<ITurretsProceeder>()                 .To<TurretsProceeder>()                     .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()         .To<TrapsIncreasingProceeder>()             .AsSingle();
            Container.Bind<IPortalsProceeder>()                 .To<PortalsProceeder>()                     .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()        .To<ShredingerBlocksProceeder>()            .AsSingle();
            Container.Bind<ISpringboardProceeder>()             .To<SpringboardProceeder>()                 .AsSingle();
            Container.Bind<IGameTimeProvider>()                 .FromComponentsInNewPrefab(gameTimeProvider).AsSingle();
            Container.Bind<IUiTimeProvider>()                   .FromComponentsInNewPrefab(uiTimeProvider)  .AsCached();
            
            #endregion
            
            #region view debug

            Container.Bind<IMazeItemsCreator>()                 .To<MazeItemsCreatorProt>()                 .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeCommon>()                   .To<ViewMazeCommonProt>()                   .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeTransitioner>()             .To<ViewMazeTransitionerFake>()             .AsSingle().When(_ => prot);
            Container.Bind<IViewCharacter>()                    .To<ViewCharacterProt>()                    .AsSingle().When(_ => prot);
            Container.Bind<IViewCharacterEffector>()            .To<ViewCharacterEffectorProt>()            .AsSingle().When(_ => prot);
            Container.Bind<IViewCharacterTail>()                .To<ViewCharacterTailProt>()                .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeMovingItemsGroup>()         .To<ViewMazeMovingItemsGroupProt>()         .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeShredingerBlocksGroup>()    .To<ViewMazeShredingerBlocksGroupProt>()    .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeSpringboardItemsGroup>()    .To<ViewMazeSpringboardItemsGroupProt>()    .AsSingle().When(_ => prot);
            Container.Bind<IViewMazePortalsGroup>()             .To<ViewMazePortalsGroupProt>()             .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeTrapsReactItemsGroup>()     .To<ViewMazeTrapsReactItemsGroupProt>()     .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeTurretsGroup>()             .To<ViewMazeTurretsGroupProt>()             .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeTrapsIncreasingItemsGroup>().To<ViewMazeTrapsIncreasingItemsGroupProt>().AsSingle().When(_ => prot);
            
            
            Container.Bind<IViewMazeItemPath>()                 .To<ViewMazeItemPathProtFake>()             .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemTurret>()               .To<ViewMazeItemTurretProtFake>()           .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemShredingerBlock>()      .To<ViewMazeItemShredingerBlockProtFake>()  .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemGravityBlock>()         .To<ViewMazeItemGravityBlockProtFake>()     .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemMovingTrap>()           .To<ViewMazeItemMovingTrapProtFake>()       .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemSpringboard>()          .To<ViewMazeItemSpringboardProtFake>()      .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemPortal>()               .To<ViewMazeItemPortalProtFake>()           .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemGravityTrap>()          .To<ViewMazeItemGravityTrapProtFake>()      .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemTrapReact>()            .To<ViewMazeItemTrapReactProtFake>()        .AsSingle().When(_ => prot);
            Container.Bind<IViewMazeItemTrapIncreasing>()       .To<ViewMazeItemTrapIncreasingProtFake>()   .AsSingle().When(_ => prot);
            
            Container.Bind<IViewMazeRotation>()                 .To<ViewMazeRotationProt>()                 .AsSingle();//.When(_ => prototyping);
            Container.Bind<IInputConfigurator>()                .To<RazorMazeInputConfiguratorProt>()       .AsSingle();//.When(_ => prototyping);
            Container.Bind<IViewUI>()                           .To<ViewUIProt>()                           .AsSingle();//.When(_ => prototyping);
            
            #endregion
            
            #region view release
            
            Container.Bind<IMazeItemsCreator>()                 .To<MazeItemsCreator>()                     .AsSingle().When(_ => release);
            Container.Bind<IViewMazeCommon>()                   .To<ViewMazeCommon>()                       .AsSingle().When(_ => release);
            Container.Bind<IViewMazeTransitioner>()             .To<ViewMazeTransitioner>()                 .AsSingle().When(_ => release);
            Container.Bind<IViewCharacter>()                    .To<ViewCharacter>()                        .AsSingle().When(_ => release);
            Container.Bind<IViewCharacterEffector>()            .To<ViewCharacterEffectorParticles>()       .AsSingle().When(_ => release);
            Container.Bind<IViewCharacterTail>()                .To<ViewCharacterTailSimple>()              .AsSingle().When(_ => release);
            Container.Bind<ITurretBulletTail>()                 .To<TurretBulletTailSimple>()               .AsSingle().When(_ => release);
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
            
            // Container.Bind<IViewMazeRotation>()              .To<ViewMazeRotation>()                     .AsSingle().When(_ => release);
            // Container.Bind<IViewUI>()                        .To<ViewUI>()                               .AsSingle().When(_ => release);

            #endregion
            
            #region device
            
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Container.Bind<IInputConfigurator>()                 .To<RazorMazeInputConfigurator>()            .AsSingle();
#endif
            #endregion
        }
    }
}
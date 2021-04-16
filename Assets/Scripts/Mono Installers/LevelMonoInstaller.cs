using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using Games.RazorMaze.Views.Views;
using TimeProviders;
using UnityEngine;
using Utils;
using Zenject;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstaller
    {
        public ModelSettings modelSettings;
        public ViewSettings viewSettings;
        public Object gameTimeProvider;
        public Object uiTimeProvider;
        
        public override void InstallBindings()
        {
            bool prototyping = GameClientUtils.GameMode == (int) EGameMode.Prototyping;
            bool release = GameClientUtils.GameMode == (int) EGameMode.Release;

            #region model
            
            Container.Bind<ModelSettings>()                        .FromScriptableObject(modelSettings)           .AsSingle();
            Container.Bind<ViewSettings>()                         .FromScriptableObject(viewSettings)            .AsSingle();
            Container.Bind<IViewGame>()                            .To<ViewGame>()                                .AsSingle();
            Container.Bind<IGameManager>()                         .To<RazorMazeGameManager>()                    .AsSingle();
            Container.Bind<IModelMazeData>()                       .To<ModelMazeData>()                           .AsSingle();
            Container.Bind<IModelMazeRotation>()                   .To<ModelMazeRotation>()                       .AsSingle();
            Container.Bind<IModelCharacter>()                      .To<ModelCharacter>()                          .AsSingle();
            Container.Bind<IModelGame>()                           .To<ModelGame>()                               .AsSingle();
            Container.Bind<ILevelStagingModel>()                   .To<LevelStagingModelDefault>()                .AsSingle();
            Container.Bind<IScoringModel>()                        .To<ScoringModelDefault>()                     .AsSingle();
            Container.Bind<IInputScheduler>()                      .To<InputScheduler>()                          .AsSingle();
            Container.Bind<ICoordinateConverter>()                 .To<CoordinateConverter>()                     .AsSingle();
            Container.Bind<IContainersGetter>()                    .To<ContainersGetter>()                        .AsSingle();
            Container.Bind<IMovingItemsProceeder>()                .To<MovingItemsProceeder>()                    .AsSingle();
            Container.Bind<IGravityItemsProceeder>()               .To<GravityItemsProceeder>()                   .AsSingle();
            Container.Bind<ITrapsReactProceeder>()                 .To<TrapsReactProceeder>()                     .AsSingle();
            Container.Bind<ITurretsProceeder>()                    .To<TurretsProceeder>()                        .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()            .To<TrapsIncreasingProceeder>()                .AsSingle();
            Container.Bind<IPortalsProceeder>()                    .To<PortalsProceeder>()                        .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()           .To<ShredingerBlocksProceeder>()               .AsSingle();
            Container.Bind<ISpringboardProceeder>()                .To<SpringboardProceeder>()                    .AsSingle();
            Container.Bind<IMazeItemsCreator>()                    .To<MazeItemsCreator>()                        .AsSingle();
            Container.Bind<IGameTimeProvider>()                    .FromComponentsInNewPrefab(gameTimeProvider)   .AsSingle();
            Container.Bind<IUiTimeProvider>()                      .FromComponentsInNewPrefab(uiTimeProvider)     .AsCached();
            
            #endregion


            #region view debug

            Container.Bind<IViewCharacter>()                        .To<ViewCharacterProt>()                      .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeMovingItemsGroup>()             .To<ViewMazeMovingItemsGroupProt>()           .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeShredingerBlocksGroup>()        .To<ViewMazeShredingerBlocksGroupProt>()      .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeSpringboardItemsGroup>()        .To<ViewMazeSpringboardItemsGroupProt>()      .AsSingle().When(_ => prototyping);
            
            Container.Bind<IViewMazeItemPath>()                     .To<ViewMazeItemPathProtFake>()                   .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeItemTurret>()                   .To<ViewMazeItemTurretProtFake>()                 .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeItemShredingerBlock>()          .To<ViewMazeItemShredingerBlockProtFake>()        .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeItemGravityBlock>()             .To<ViewMazeItemGravityBlockProtFake>()           .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeItemMovingTrap>()               .To<ViewMazeItemMovingTrapProtFake>()             .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeItemSpringboard>()              .To<ViewMazeItemSpringboardProtFake>()       .AsSingle().When(_ => prototyping);
            
            Container.Bind<IViewMazeCommon>()                       .To<ViewMazeCommonProt>()                     .AsSingle();
            Container.Bind<IViewMazeRotation>()                     .To<ViewMazeRotationProt>()                   .AsSingle();
            Container.Bind<IViewMazeTrapsReactItemsGroup>()         .To<ViewMazeTrapsReactItemsGroupProt>()       .AsSingle();
            Container.Bind<IViewMazeTrapsIncreasingItemsGroup>()    .To<ViewMazeTrapsIncreasingItemsGroupProt>()  .AsSingle();
            Container.Bind<IViewMazeTurretsGroup>()                 .To<ViewMazeTurretsGroupProt>()               .AsSingle();
            Container.Bind<IViewMazePortalsGroup>()                 .To<ViewMazePortalsGroupProt>()               .AsSingle();
            Container.Bind<IViewUI>()                               .To<ViewUIProt>()                             .AsSingle();
            Container.Bind<IInputConfigurator>()                    .To<RazorMazeInputConfiguratorProt>()         .AsSingle();
            
            #endregion
            
            #region view release
            
            Container.Bind<IViewCharacter>()                        .To<ViewCharacter>()                          .AsSingle().When(_ => release);
            Container.Bind<IViewCharacterTail>()                    .To<ViewCharacterTailSimple>()                .AsSingle().When(_ => release);
            Container.Bind<ITurretBulletTail>()                     .To<TurretBulletTailSimple>()                 .AsSingle().When(_ => release);
            Container.Bind<IViewMazeMovingItemsGroup>()             .To<ViewMazeMovingItemsGroup>()               .AsSingle().When(_ => release);
            Container.Bind<IViewMazeShredingerBlocksGroup>()        .To<ViewMazeShredingerBlocksGroup>()          .AsSingle().When(_ => release);
            Container.Bind<IViewMazeTurretsGroup>()                 .To<ViewMazeTurretsGroup>()                   .AsSingle().When(_ => release);
            Container.Bind<IViewMazeSpringboardItemsGroup>()        .To<ViewMazeSpringboardItemsGroup>()          .AsSingle().When(_ => release);
            
            Container.Bind<IViewMazeItemPath>()                     .To<ViewMazeItemPath>()                       .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemGravityBlock>()             .To<ViewMazeItemGravityBlock>()               .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemMovingTrap>()               .To<ViewMazeItemMovingTrap>()                 .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemShredingerBlock>()          .To<ViewMazeItemShredingerBlock>()            .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemTurret>()                   .To<ViewMazeItemTurret>()                     .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemSpringboard>()              .To<ViewMazeItemSpringboard>()                .AsSingle().When(_ => release);

            #endregion

        }
    }
}
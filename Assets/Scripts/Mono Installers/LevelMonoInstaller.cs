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
using UnityEngine;
using Utils;
using Zenject;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstaller
    {
        public ScriptableObject modelSettings;
        
        public override void InstallBindings()
        {
            bool prototyping = GameClientUtils.GameMode == (int) EGameMode.Prototyping;
            bool release = GameClientUtils.GameMode == (int) EGameMode.Release;

            #region model
            
            Container.Bind<ModelSettings>().FromScriptableObject(modelSettings).AsSingle();
            Container.Bind<IViewGame>()                         .To<ViewGame>()                             .AsSingle();
            Container.Bind<IGameManager>()                      .To<RazorMazeGameManager>()                 .AsSingle();
            Container.Bind<IModelMazeData>()                    .To<ModelMazeData>()                        .AsSingle();
            Container.Bind<IModelMazeRotation>()                .To<ModelMazeRotation>()                    .AsSingle();
            Container.Bind<IModelCharacter>()                   .To<ModelCharacter>()                       .AsSingle();
            Container.Bind<IModelGame>()                        .To<ModelGame>()                            .AsSingle();
            Container.Bind<ILevelStagingModel>()                .To<LevelStagingModelDefault>()             .AsSingle();
            Container.Bind<IScoringModel>()                     .To<ScoringModelDefault>()                  .AsSingle();
            Container.Bind<IInputScheduler>()                   .To<InputScheduler>()                       .AsSingle();
            Container.Bind<ICoordinateConverter>()              .To<CoordinateConverter>()                  .AsSingle();
            Container.Bind<IContainersGetter>()                 .To<ContainersGetter>()                     .AsSingle();
            Container.Bind<IMovingItemsProceeder>()             .To<MovingItemsProceeder>()                 .AsSingle();
            Container.Bind<IGravityItemsProceeder>()            .To<GravityItemsProceeder>()                .AsSingle();
            Container.Bind<ITrapsReactProceeder>()              .To<TrapsReactProceeder>()                  .AsSingle();
            Container.Bind<ITurretsProceeder>()                 .To<TurretsProceeder>()                     .AsSingle();
            Container.Bind<ITrapsIncreasingProceeder>()         .To<TrapsIncreasingProceeder>()             .AsSingle();
            Container.Bind<IPortalsProceeder>()                 .To<PortalsProceeder>()                     .AsSingle();
            Container.Bind<IShredingerBlocksProceeder>()        .To<ShredingerBlocksProceeder>()            .AsSingle();
            Container.Bind<ISpringboardProceeder>()             .To<SpringboardProceeder>()                 .AsSingle();
            Container.Bind<IMazeItemsCreator>()                 .To<MazeItemsCreator>()                     .AsSingle();
            
            #endregion


            #region view debug

            Container.Bind<IViewCharacter>()                    .To<ViewCharacterProt>()                    .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeCommon>()                   .To<ViewMazeCommonProt>()                   .AsSingle();
            Container.Bind<IViewMazeRotation>()                 .To<ViewMazeRotationProt>()                 .AsSingle();
            Container.Bind<IViewMazeMovingItemsGroup>()         .To<ViewMazeMovingItemsGroupProt>()         .AsSingle();
            Container.Bind<IViewMazeTrapsReactItemsGroup>()     .To<ViewMazeTrapsReactItemsGroupProt>()     .AsSingle();
            Container.Bind<IViewMazeTrapsIncreasingItemsGroup>().To<ViewMazeTrapsIncreasingItemsGroupProt>().AsSingle();
            Container.Bind<IViewMazeTurretsGroup>()             .To<ViewMazeTurretsGroupProt>()             .AsSingle();
            Container.Bind<IViewMazePortalsGroup>()             .To<ViewMazePortalsGroupProt>()             .AsSingle();
            Container.Bind<IViewMazeShredingerBlocksGroup>()    .To<ViewMazeShredingerBlocksGroupProt>()    .AsSingle();
            Container.Bind<IViewMazeSpringboardItemsGroup>()    .To<ViewMazeSpringboardItemsGroupProt>()    .AsSingle();
            Container.Bind<IViewUI>()                           .To<ViewUIProt>()                           .AsSingle();
            Container.Bind<IInputConfigurator>()                .To<RazorMazeInputConfiguratorProt>()       .AsSingle();

            //Container.Bind<IViewMazeItemPath>()                 .To<ViewMazeItemPathProt>()                 .AsSingle().When(_ => release);
            #endregion
            
            #region view release
            
            Container.Bind<IViewCharacter>()                    .To<ViewCharacter>()                        .AsSingle().When(_ => release);
            Container.Bind<IViewMazeItemPath>()                 .To<ViewMazeItemPath>()                     .AsSingle().When(_ => release);


            #endregion

        }
    }
}
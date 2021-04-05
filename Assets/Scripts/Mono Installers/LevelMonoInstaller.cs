using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItemGroups;
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
            
            Container.Bind<IViewMazeCommon>()                   .To<ViewMazeCommonProt>()                   .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeRotation>()                 .To<ViewMazeRotationProt>()                 .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeMovingItemsGroup>()         .To<ViewMazeMovingItemsGroupProt>()         .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeTrapsReactItemsGroup>()     .To<ViewMazeTrapsReactItemsGroupProt>()     .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeTrapsIncreasingItemsGroup>().To<ViewMazeTrapsIncreasingItemsGroupProt>().AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeTurretsGroup>()             .To<ViewMazeTurretsGroupProt>()             .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazePortalsGroup>()             .To<ViewMazePortalsGroupProt>()             .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeShredingerBlocksGroup>()    .To<ViewMazeShredingerBlocksGroupProt>()    .AsSingle().When(_ => prototyping);
            Container.Bind<IViewUI>()                           .To<ViewUIProt>()                           .AsSingle().When(_ => prototyping);
            Container.Bind<IViewCharacter>()                    .To<ViewCharacterProt>()                    .AsSingle().When(_ => prototyping);
            Container.Bind<IInputConfigurator>()                .To<RazorMazeInputConfiguratorProt>()       .AsSingle().When(_ => prototyping);
        }
    }
}
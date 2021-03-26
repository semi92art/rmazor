using Games;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using Utils;
using Zenject;

namespace Mono_Installers
{
    public class LevelMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            bool prototyping = GameClientUtils.GameMode == (int) EGameMode.Prototyping;
            
            Container.Bind<RazorMazeModelSettings>()       .FromComponentInHierarchy()            .AsSingle();
            Container.Bind<IGameManager>()                 .To<RazorMazeGameManager>()            .AsSingle();
            Container.Bind<IMazeModel>()                   .To<MazeModel>()                       .AsSingle();
            Container.Bind<ICharacterModel>()              .To<CharacterModel>()                  .AsSingle();
            Container.Bind<IModelGame>()                   .To<ModelGame>()                       .AsSingle();
            Container.Bind<ILevelStagingModel>()           .To<LevelStagingModelDefault>()        .AsSingle();
            Container.Bind<IScoringModel>()                .To<ScoringModelDefault>()             .AsSingle();
            Container.Bind<IInputScheduler>()              .To<InputScheduler>()                  .AsSingle();
            Container.Bind<ICoordinateConverter>()         .To<CoordinateConverter>()             .AsSingle();
            Container.Bind<IContainersGetter>()            .To<ContainersGetter>()                .AsSingle();
            Container.Bind<IMazeMovingItemsProceeder>()    .To<MazeMovingItemsProceeder>()        .AsSingle();
            Container.Bind<IMazeTrapsReactProceeder>()     .To<MazeTrapsReactProceeder>()         .AsSingle();
            Container.Bind<ICharacterMover>()              .To<CharacterMover>()                  .AsSingle();
            Container.Bind<IViewGame>()                    .To<ViewGame>()                        .AsSingle();
            
            Container.Bind<IViewMazeCommon>()              .To<ViewMazeCommonProt>()              .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeRotation>().To<ViewMazeRotationProt>().AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeMovingItemsGroup>()    .To<ViewMazeMovingItemsGroupProt>()    .AsSingle().When(_ => prototyping);
            Container.Bind<IViewMazeTrapsReactItemsGroup>().To<ViewMazeTrapsReactItemsGroupProt>().AsSingle().When(_ => prototyping);
            Container.Bind<IViewUI>()                      .To<ViewUIProt>()                      .AsSingle().When(_ => prototyping);
            Container.Bind<IViewCharacter>()               .To<ViewCharacterProt>()               .AsSingle().When(_ => prototyping);
            Container.Bind<IInputConfigurator>()           .To<RazorMazeInputConfiguratorProt>()  .AsSingle().When(_ => prototyping);
        }
    }
}
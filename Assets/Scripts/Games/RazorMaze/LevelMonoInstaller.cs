using Games;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using Utils;
using Zenject;

public class LevelMonoInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        bool razorMaze = GameClientUtils.GameId == 1;
        bool prototyping = GameClientUtils.GameMode == (int) EGameMode.Prototyping;
        Container.BindInstance(new MazeInfo());
        Container.Bind<IGameManager>()      .To<RazorMazeGameManager>()          .AsSingle().When(_ => razorMaze);
        Container.Bind<IMazeModel>()        .To<MazeModel>()                     .AsSingle().When(_ => razorMaze);
        Container.Bind<ICharacterModel>()   .To<CharacterModel>()                .AsSingle().When(_ => razorMaze);
        Container.Bind<IGameModel>()        .To<GameModel>()                     .AsSingle().When(_ => razorMaze);
        Container.Bind<ILevelStagingModel>().To<LevelStagingModelDefault>()      .AsSingle().When(_ => razorMaze);
        Container.Bind<IScoringModel>()     .To<ScoringModelDefault>()           .AsSingle().When(_ => razorMaze);
        Container.Bind<IInputScheduler>()   .To<InputScheduler>()                .AsSingle().When(_ => razorMaze);
        
        Container.Bind<IMazeView>()         .To<MazeViewProt>()                  .AsSingle().When(_ => razorMaze && prototyping);
        Container.Bind<IGameUiView>()       .To<GameUiViewProt>()                .AsSingle().When(_ => razorMaze && prototyping);
        Container.Bind<ICharacterView>()    .To<CharacterViewProt>()             .AsSingle().When(_ => razorMaze && prototyping);
        Container.Bind<IInputConfigurator>().To<RazorMazeInputConfiguratorProt>().AsSingle().When(_ => razorMaze && prototyping);

    }
}
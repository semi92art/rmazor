using Games;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using Utils;
using Zenject;

public class DIConfiguratorLevel : MonoInstaller
{
    public override void InstallBindings()
    {
        switch (GameClientUtils.GameId)
        {
            case 1:
                Container.Bind<IGameManager>().To<RazorMazeGameManager>().AsSingle();
                InstallBindingsGame1();
                break;
        }
    }

    private void InstallBindingsGame1()
    {
        Container.Bind<IMazeModel>().To<MazeModelDefault>().AsSingle();
        Container.Bind<ICharacterModel>().To<CharacherModelDefault>().AsSingle();
        Container.Bind<IGameModel>().To<GameModelDefault>().AsSingle();
        Container.Bind<ILevelStagingModel>().To<LevelStagingModelDefault>().AsSingle();
        Container.Bind<IScoringModel>().To<ScoringModelDefault>().AsSingle();
        Container.BindInstance(new MazeInfo());
        
        switch (GameClientUtils.GameMode)
        {
            case (int)EGameMode.Prototyping:
                Container.Bind<IMazeView>().To<MazeViewProt>().AsSingle();
                Container.Bind<IGameUiView>().To<GameUiViewProt>().AsSingle();
                Container.Bind<ICharacterView>().To<CharacterViewProt>().AsSingle();
                Container.Bind<IInputConfigurator>().To<RazorMazeInputConfiguratorProt>().AsSingle();
                break;
        }
    }
}
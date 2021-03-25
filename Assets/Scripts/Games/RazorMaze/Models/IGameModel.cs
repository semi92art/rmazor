namespace Games.RazorMaze.Models
{
    public interface IGameModel
    {
        IMazeModel Maze { get; }
        IMazeTransformer MazeTransformer { get; }
        ICharacterModel Character { get; }
        ILevelStagingModel LevelStaging { get; }
        IScoringModel Scoring { get; }
        IInputScheduler InputScheduler { get; }
    }
}
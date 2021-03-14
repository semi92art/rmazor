namespace Games.RazorMaze.Models
{
    public interface IGameModel
    {
        IMazeModel Maze { get; }
        ICharacterModel Character { get; }
        ILevelStagingModel LevelStaging { get; }
        IScoringModel Scoring { get; }
    }
}
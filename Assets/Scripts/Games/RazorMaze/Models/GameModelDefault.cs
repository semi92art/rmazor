namespace Games.RazorMaze.Models
{
    public class GameModelDefault : IGameModel
    {
        public IMazeModel Maze { get; }
        public ICharacterModel Character { get; }
        public ILevelStagingModel LevelStaging { get; }
        public IScoringModel Scoring { get; }
        
        public GameModelDefault(
            IMazeModel _Model,
            ICharacterModel _CharacterModel,
            ILevelStagingModel _StagingModel,
            IScoringModel _ScoringModel)
        {
            Maze = _Model;
            Character = _CharacterModel;
            LevelStaging = _StagingModel;
            Scoring = _ScoringModel;

            Maze.OnMazeChanged += Character.UpdateMazeInfo;
        }
    }
}
using Exceptions;
using Utils;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IPreInit
    {
        IMazeModel Maze { get; }
        IMazeMovingItemsProceeder MazeMovingItemsProceeder { get; }
        IMazeTrapsReactProceeder  MazeTrapsReactProceeder { get; }
        ICharacterModel Character { get; }
        ILevelStagingModel LevelStaging { get; }
        IScoringModel Scoring { get; }
        IInputScheduler InputScheduler { get; }
    }
    
    public class ModelGame : IModelGame
    {
        public IMazeModel                Maze { get; }
        public IMazeMovingItemsProceeder MazeMovingItemsProceeder { get; }
        public IMazeTrapsReactProceeder  MazeTrapsReactProceeder { get; }
        public ICharacterModel           Character { get; }
        public ILevelStagingModel        LevelStaging { get; }
        public IScoringModel             Scoring { get; }
        public IInputScheduler           InputScheduler { get; }
        
        
        private ICharacterModelFull      CharacterFull { get; }
        
        public ModelGame(
            IMazeModel                _Model,
            IMazeMovingItemsProceeder _MazeMovingItemsProceeder,
            ICharacterModel           _CharacterModel,
            ILevelStagingModel        _StagingModel,
            IScoringModel             _ScoringModel,
            IInputScheduler           _InputScheduler,
            IMazeTrapsReactProceeder  _MazeTrapsReactProceeder)
        {
            Maze                                  = _Model;
            MazeMovingItemsProceeder              = _MazeMovingItemsProceeder;
            MazeTrapsReactProceeder               = _MazeTrapsReactProceeder;
            Character                             = _CharacterModel;
            LevelStaging                          = _StagingModel;
            Scoring                               = _ScoringModel;
            InputScheduler                        = _InputScheduler;

            CharacterFull                         = _CharacterModel as ICharacterModelFull;
        }
        
        public void PreInit()
        {
            Maze.MazeChanged                      += MazeOnMazeChanged;
            Maze.RotationStarted                  += MazeOnRotationStarted;
            Maze.RotationFinished                 += MazeOnRotationFinished;
            Character.MoveContinued               += CharacterOnMoveContinued;
            Character.MoveFinished                += CharacterOnFinishMove; 
            InputScheduler.MoveCommand            += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand          += InputSchedulerOnRotateCommand;
            
            Character.PreInit();
        }
        
        public void Init()
        {
            Character.Init();
        }
        
        private void MazeOnMazeChanged(MazeInfo _Info, MazeOrientation _Orientation) => CharacterFull.OnMazeInfoUpdated(_Info, _Orientation);
        private void MazeOnRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation) => CharacterFull.OnMazeInfoUpdated(Maze.Info, _Orientation);
        private void MazeOnRotationFinished() => InputScheduler.UnlockRotation();
        

        private void CharacterOnMoveContinued(CharacterMovingEventArgs _Args)
        {
            MazeMovingItemsProceeder.OnCharacterMoveContinued(_Args, Maze.Orientation);
            MazeTrapsReactProceeder.OnCharacterMoveContinued(_Args);
        }
        
        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args) => InputScheduler.UnlockMovement();
        
        private void InputSchedulerOnMoveCommand(EInputCommand _Command)
        {
            Dbg.Log("Move:" + _Command);
            MazeMoveDirection dir = default;
            switch (_Command)
            {
                case EInputCommand.MoveUp:    dir = MazeMoveDirection.Up;    break;
                case EInputCommand.MoveDown:  dir = MazeMoveDirection.Down;  break;
                case EInputCommand.MoveLeft:  dir = MazeMoveDirection.Left;  break;
                case EInputCommand.MoveRight: dir = MazeMoveDirection.Right; break;
                case EInputCommand.RotateClockwise:
                case EInputCommand.RotateCounterClockwise:
                    break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            Character.Move(dir);
        }
        
        private void InputSchedulerOnRotateCommand(EInputCommand _Command)
        {
            MazeRotateDirection dir;
            switch (_Command)
            {
                case EInputCommand.RotateClockwise:        dir = MazeRotateDirection.Clockwise;        break;
                case EInputCommand.RotateCounterClockwise: dir = MazeRotateDirection.CounterClockwise; break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            Maze.Rotate(dir);
        }
    }
}
using Exceptions;

namespace Games.RazorMaze.Models
{
    public class GameModel : IGameModel
    {
        public IMazeModel                Maze { get; }
        public IMazeTransformer          MazeTransformer { get; }
        public ICharacterModel           Character { get; }
        public ILevelStagingModel        LevelStaging { get; }
        public IScoringModel             Scoring { get; }
        public IInputScheduler           InputScheduler { get; }
        
        
        private ICharacterModelFull      CharacterFull { get; }
        
        public GameModel(
            IMazeModel            _Model,
            IMazeTransformer      _MazeTransformer,
            ICharacterModel       _CharacterModel,
            ILevelStagingModel    _StagingModel,
            IScoringModel         _ScoringModel,
            IInputScheduler       _InputScheduler)
        {
            Maze                                  = _Model;
            MazeTransformer                       = _MazeTransformer;
            Character                             = _CharacterModel;
            LevelStaging                          = _StagingModel;
            Scoring                               = _ScoringModel;
            InputScheduler                        = _InputScheduler;

            CharacterFull = _CharacterModel as ICharacterModelFull;
            
            Maze.MazeChanged                      += MazeOnMazeChanged;
            Maze.RotationStarted                  += MazeOnRotationStarted;
            Maze.RotationFinished                 += MazeOnRotationFinished;
            Character.MoveContinued += CharacterOnMoveContinued;
            Character.MoveFinished                += CharacterOnFinishMove; 
            InputScheduler.MoveCommand            += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand          += InputSchedulerOnRotateCommand;
        }

        private void CharacterOnMoveContinued(CharacterMovingEventArgs _Args)
        {
            MazeTransformer.OnCharacterMoveContinued(_Args, Maze.Orientation);
        }

        private void MazeOnMazeChanged(MazeInfo _Info, MazeOrientation _Orientation)
        {
            CharacterFull.OnMazeInfoUpdated(_Info, _Orientation);
        }

        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args)
        {
            // MazeTransformer.OnCharacterMoved(_Args);
            InputScheduler.UnlockMovement();
        }
        
        private void MazeOnRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            CharacterFull.OnMazeInfoUpdated(Maze.Info, _Orientation);
        }
        
        private void MazeOnRotationFinished()
        {
            InputScheduler.UnlockRotation();
        }

        private void InputSchedulerOnMoveCommand(EInputCommand _Command)
        {
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
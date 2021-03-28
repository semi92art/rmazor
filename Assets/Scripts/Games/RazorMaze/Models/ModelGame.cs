using Exceptions;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IPreInit
    {
        IModelMazeData                Data { get; }
        IModelMazeRotation            MazeRotation { get; }
        IMazeMovingItemsProceeder     MovingItemsProceeder { get; }
        IMazeGravityItemsProceeder    GravityItemsProceeder { get; }
        IMazeTrapsReactProceeder      TrapsReactProceeder { get; }
        IMazeTrapsIncreasingProceeder TrapsIncreasingProceeder { get; }
        IMazeTurretsProceeder         TurretsProceeder { get; }
        IModelCharacter               Character { get; }
        ILevelStagingModel            LevelStaging { get; }
        IScoringModel                 Scoring { get; }
        IInputScheduler               InputScheduler { get; }
    }
    
    public class ModelGame : IModelGame
    {
        public IModelMazeData                Data { get; }
        public IModelMazeRotation            MazeRotation { get; }
        public IMazeMovingItemsProceeder     MovingItemsProceeder { get; }
        public IMazeGravityItemsProceeder    GravityItemsProceeder { get; }
        public IMazeTrapsReactProceeder      TrapsReactProceeder { get; }
        public IMazeTrapsIncreasingProceeder TrapsIncreasingProceeder { get; }
        public IMazeTurretsProceeder         TurretsProceeder { get; }
        public IModelCharacter               Character { get; }
        public ILevelStagingModel            LevelStaging { get; }
        public IScoringModel                 Scoring { get; }
        public IInputScheduler               InputScheduler { get; }
        
        public ModelGame(
            IModelMazeData                _Data,
            IModelMazeRotation            _MazeRotation,
            IMazeMovingItemsProceeder     _MovingItemsProceeder,
            IMazeGravityItemsProceeder    _GravityItemsProceeder,
            IMazeTrapsReactProceeder      _TrapsReactProceeder,
            IMazeTrapsIncreasingProceeder _TrapsIncreasingProceeder,
            IMazeTurretsProceeder         _TurretsProceeder,
            IModelCharacter               _CharacterModel,
            ILevelStagingModel            _StagingModel,
            IScoringModel                 _ScoringModel,
            IInputScheduler               _InputScheduler)
        {
            Data                                  = _Data;
            MazeRotation                          = _MazeRotation;
            MovingItemsProceeder                  = _MovingItemsProceeder;
            GravityItemsProceeder                 = _GravityItemsProceeder;
            TrapsReactProceeder                   = _TrapsReactProceeder;
            TrapsIncreasingProceeder              = _TrapsIncreasingProceeder;
            TurretsProceeder                      = _TurretsProceeder;
            Character                             = _CharacterModel;
            LevelStaging                          = _StagingModel;
            Scoring                               = _ScoringModel;
            InputScheduler                        = _InputScheduler;
        }
        
        public void PreInit()
        {
            Data.MazeChanged                      += MazeOnMazeChanged;
            MazeRotation.RotationStarted          += MazeOnRotationStarted;
            MazeRotation.RotationFinished         += MazeOnRotationFinished;
            Character.CharacterMoveContinued      += CharacterOnMoveContinued;
            Character.CharacterMoveFinished       += CharacterOnFinishMove; 
            InputScheduler.MoveCommand            += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand          += InputSchedulerOnRotateCommand;
            Character.PreInit();
        }
        
        public void Init()
        {
            Character.Init();
        }

        private void MazeOnMazeChanged(MazeInfo _Info)
        {
            Character.OnMazeChanged(_Info);
            MovingItemsProceeder.OnMazeChanged(_Info);
            GravityItemsProceeder.OnMazeChanged(_Info);
            TrapsReactProceeder.OnMazeChanged(_Info);
            TrapsIncreasingProceeder.OnMazeChanged(_Info);
            TurretsProceeder.OnMazeChanged(_Info);
        }

        private void MazeOnRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            //Character.OnMazeChanged(Data.Info);  
        }

        private void MazeOnRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            InputScheduler.UnlockRotation();
            GravityItemsProceeder.OnMazeOrientationChanged();
        }
        

        private void CharacterOnMoveContinued(CharacterMovingEventArgs _Args)
        {
            GravityItemsProceeder.OnCharacterMoveContinued(_Args);
            TrapsReactProceeder.OnCharacterMoveContinued(_Args);
            TrapsIncreasingProceeder.OnCharacterMoveContinued(_Args);
        }
        
        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args) => InputScheduler.UnlockMovement();
        
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
            MazeRotation.Rotate(dir);
        }
    }
}
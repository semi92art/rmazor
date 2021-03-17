using Entities;
using Exceptions;
using UnityEngine.EventSystems;

namespace Games.RazorMaze.Models
{
    public class GameModel : IGameModel
    {
        public IMazeModel                Maze { get; }
        public ICharacterModel           Character { get; }
        public ILevelStagingModel        LevelStaging { get; }
        public IScoringModel             Scoring { get; }
        public IInputScheduler           InputScheduler { get; }
        
        public GameModel(
            IMazeModel                   _Model,
            ICharacterModel              _CharacterModel,
            ILevelStagingModel           _StagingModel,
            IScoringModel                _ScoringModel,
            IInputScheduler              _InputScheduler)
        {
            Maze                         = _Model;
            Character                    = _CharacterModel;
            LevelStaging                 = _StagingModel;
            Scoring                      = _ScoringModel;
            InputScheduler               = _InputScheduler;
            Maze.MazeChanged             += MazeOnMazeChanged;
            InputScheduler.MoveCommand   += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand += InputSchedulerOnRotateCommand;
            Maze.RotationStarted         += MazeOnRotationStarted;
            Maze.RotationFinished        += MazeOnRotationFinished;
            Character.StartMove          += CharacterOnStartMove;
            Character.FinishMove         += CharacterOnFinishMove; 
        }

        private void MazeOnMazeChanged(MazeInfo _Info, MazeOrientation _Orientation)
        {
            Character.UpdateMazeInfo(_Info, _Orientation);
        }

        private void CharacterOnStartMove(V2Int _Value1, V2Int _Value2)
        {
            
        }
        
        private void CharacterOnFinishMove()
        {
            InputScheduler.UnlockMovement();
        }
        
        private void MazeOnRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            Character.UpdateMazeInfo(Maze.Info, _Orientation);
        }
        
        private void MazeOnRotationFinished()
        {
            InputScheduler.UnlockRotation();
        }

        private void InputSchedulerOnMoveCommand(EInputCommand _Command)
        {
            MoveDirection dir;
            switch (_Command)
            {
                case EInputCommand.MoveUp:    dir = MoveDirection.Up;    break;
                case EInputCommand.MoveDown:  dir = MoveDirection.Down;  break;
                case EInputCommand.MoveLeft:  dir = MoveDirection.Left;  break;
                case EInputCommand.MoveRight: dir = MoveDirection.Right; break;
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
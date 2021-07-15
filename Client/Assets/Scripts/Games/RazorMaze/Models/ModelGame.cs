﻿using Exceptions;
using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IPreInit
    {
        IModelMazeData                Data { get; }
        IModelMazeRotation            MazeRotation { get; }
        IMovingItemsProceeder         MovingItemsProceeder { get; }
        IGravityItemsProceeder        GravityItemsProceeder { get; }
        ITrapsReactProceeder          TrapsReactProceeder { get; }
        ITrapsIncreasingProceeder     TrapsIncreasingProceeder { get; }
        ITurretsProceeder             TurretsProceeder { get; }
        IPortalsProceeder             PortalsProceeder { get; }
        IShredingerBlocksProceeder    ShredingerBlocksProceeder { get; }
        ISpringboardProceeder         SpringboardProceeder { get; }
        IModelCharacter               Character { get; }
        ILevelStagingModel            LevelStaging { get; }
        IScoringModel                 Scoring { get; }
        IInputScheduler               InputScheduler { get; }
    }
    
    public class ModelGame : IModelGame
    {
        public IModelMazeData                Data { get; }
        public IModelMazeRotation            MazeRotation { get; }
        public IMovingItemsProceeder         MovingItemsProceeder { get; }
        public IGravityItemsProceeder        GravityItemsProceeder { get; }
        public ITrapsReactProceeder          TrapsReactProceeder { get; }
        public ITrapsIncreasingProceeder     TrapsIncreasingProceeder { get; }
        public ITurretsProceeder             TurretsProceeder { get; }
        public IPortalsProceeder             PortalsProceeder { get; }
        public IShredingerBlocksProceeder    ShredingerBlocksProceeder { get; }
        public ISpringboardProceeder         SpringboardProceeder { get; }
        public IModelCharacter               Character { get; }
        public ILevelStagingModel            LevelStaging { get; }
        public IScoringModel                 Scoring { get; }
        public IInputScheduler               InputScheduler { get; }
        
        public ModelGame(
            IModelMazeData                _Data,
            IModelMazeRotation            _MazeRotation,
            IMovingItemsProceeder         _MovingItemsProceeder,
            IGravityItemsProceeder        _GravityItemsProceeder,
            ITrapsReactProceeder          _TrapsReactProceeder,
            ITrapsIncreasingProceeder     _TrapsIncreasingProceeder,
            ITurretsProceeder             _TurretsProceeder,
            IPortalsProceeder             _PortalsProceeder,
            IModelCharacter               _CharacterModel,
            ILevelStagingModel            _StagingModel,
            IScoringModel                 _ScoringModel,
            IInputScheduler               _InputScheduler,
            IShredingerBlocksProceeder    _ShredingerBlocksProceeder,
            ISpringboardProceeder         _SpringboardProceeder)
        {
            Data                                  = _Data;
            MazeRotation                          = _MazeRotation;
            MovingItemsProceeder                  = _MovingItemsProceeder;
            GravityItemsProceeder                 = _GravityItemsProceeder;
            TrapsReactProceeder                   = _TrapsReactProceeder;
            TrapsIncreasingProceeder              = _TrapsIncreasingProceeder;
            TurretsProceeder                      = _TurretsProceeder;
            PortalsProceeder                      = _PortalsProceeder;
            Character                             = _CharacterModel;
            LevelStaging                          = _StagingModel;
            Scoring                               = _ScoringModel;
            InputScheduler                        = _InputScheduler;
            ShredingerBlocksProceeder             = _ShredingerBlocksProceeder;
            SpringboardProceeder                  = _SpringboardProceeder;
        }
        
        public void PreInit()
        {
            Data.MazeItemsProceedStarted          += GravityItemsProceeder.Start;
            Data.MazeItemsProceedStopped          += GravityItemsProceeder.Stop;
            Data.MazeChanged                      += MazeOnMazeChanged;
            MazeRotation.RotationFinished         += MazeOnRotationFinished;
            Character.CharacterMoveStarted        += CharacterOnMoveStarted;
            Character.CharacterMoveContinued      += CharacterOnMoveContinued;
            Character.CharacterMoveFinished       += CharacterOnFinishMove; 
            InputScheduler.MoveCommand            += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand          += InputSchedulerOnRotateCommand;
            PortalsProceeder.PortalEvent          += Character.OnPortal;
            SpringboardProceeder.SpringboardEvent += Character.OnSpringboard;
            Data.PreInit();
        }

        public void Init()
        {
            Character.Init();
        }
        
        private void CharacterOnMoveStarted(CharacterMovingEventArgs _Args)
        {
            GravityItemsProceeder.OnCharacterMoveStarted(_Args);
        }

        private void MazeOnMazeChanged(MazeInfo _Info)
        {
            foreach (var item in new IOnMazeChanged []
            {
                Character,
                MovingItemsProceeder,
                GravityItemsProceeder,
                TrapsReactProceeder,
                TrapsIncreasingProceeder,
                TurretsProceeder,
                PortalsProceeder,
                ShredingerBlocksProceeder,
                SpringboardProceeder
            })
            {
                item.OnMazeChanged(_Info);
            }
        }

        private void MazeOnRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            InputScheduler.UnlockRotation();
            GravityItemsProceeder.OnMazeOrientationChanged();
        }
        

        private void CharacterOnMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var proceeder in new ICharacterMoveContinued[]
            {
                Data,
                TrapsReactProceeder,
                TrapsIncreasingProceeder,
                PortalsProceeder,
                ShredingerBlocksProceeder,
                SpringboardProceeder
            })
            {
                proceeder.OnCharacterMoveContinued(_Args);                
            }
        }
        
        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args) => InputScheduler.UnlockMovement();
        
        private void InputSchedulerOnMoveCommand(EInputCommand _Command)
        {
            EMazeMoveDirection dir = default;
            switch (_Command)
            {
                case EInputCommand.MoveUp:    dir = EMazeMoveDirection.Up;    break;
                case EInputCommand.MoveDown:  dir = EMazeMoveDirection.Down;  break;
                case EInputCommand.MoveLeft:  dir = EMazeMoveDirection.Left;  break;
                case EInputCommand.MoveRight: dir = EMazeMoveDirection.Right; break;
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
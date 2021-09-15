using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IPreInit
    {
        IModelMazeData                            Data { get; }
        IModelMazeRotation                        MazeRotation { get; }
        IPathItemsProceeder                       PathItemsProceeder { get; }
        IMovingItemsProceeder                     MovingItemsProceeder { get; }
        IGravityItemsProceeder                    GravityItemsProceeder { get; }
        ITrapsReactProceeder                      TrapsReactProceeder { get; }
        ITrapsIncreasingProceeder                 TrapsIncreasingProceeder { get; }
        ITurretsProceeder                         TurretsProceeder { get; }
        IPortalsProceeder                         PortalsProceeder { get; }
        IShredingerBlocksProceeder                ShredingerBlocksProceeder { get; }
        ISpringboardProceeder                     SpringboardProceeder { get; }
        IModelCharacter                           Character { get; }
        ILevelStagingModel                        LevelStaging { get; }
        IInputScheduler                           InputScheduler { get; }
    }
    
    public class ModelGame : IModelGame
    {
        #region api
        
        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler Initialized;
        
        public IModelMazeData                     Data { get; }
        public IModelMazeRotation                 MazeRotation { get; }
        public IPathItemsProceeder                PathItemsProceeder { get; }
        public IMovingItemsProceeder              MovingItemsProceeder { get; }
        public IGravityItemsProceeder             GravityItemsProceeder { get; }
        public ITrapsReactProceeder               TrapsReactProceeder { get; }
        public ITrapsIncreasingProceeder          TrapsIncreasingProceeder { get; }
        public ITurretsProceeder                  TurretsProceeder { get; }
        public IPortalsProceeder                  PortalsProceeder { get; }
        public IShredingerBlocksProceeder         ShredingerBlocksProceeder { get; }
        public ISpringboardProceeder              SpringboardProceeder { get; }
        public IModelCharacter                    Character { get; }
        public ILevelStagingModel                 LevelStaging { get; }
        public IInputScheduler                    InputScheduler { get; }
        
        public ModelGame(
            IModelMazeData                        _Data,
            IModelMazeRotation                    _MazeRotation,
            IPathItemsProceeder                   _PathItemsProceeder,
            IMovingItemsProceeder                 _MovingItemsProceeder,
            IGravityItemsProceeder                _GravityItemsProceeder,
            ITrapsReactProceeder                  _TrapsReactProceeder,
            ITrapsIncreasingProceeder             _TrapsIncreasingProceeder,
            ITurretsProceeder                     _TurretsProceeder,
            IPortalsProceeder                     _PortalsProceeder,
            IModelCharacter                       _CharacterModel,
            ILevelStagingModel                    _StagingModel,
            IInputScheduler                       _InputScheduler,
            IShredingerBlocksProceeder            _ShredingerBlocksProceeder,
            ISpringboardProceeder                 _SpringboardProceeder)
        {
            Data                                  = _Data;
            MazeRotation                          = _MazeRotation;
            PathItemsProceeder                    = _PathItemsProceeder;
            MovingItemsProceeder                  = _MovingItemsProceeder;
            GravityItemsProceeder                 = _GravityItemsProceeder;
            TrapsReactProceeder                   = _TrapsReactProceeder;
            TrapsIncreasingProceeder              = _TrapsIncreasingProceeder;
            TurretsProceeder                      = _TurretsProceeder;
            PortalsProceeder                      = _PortalsProceeder;
            Character                             = _CharacterModel;
            LevelStaging                          = _StagingModel;
            InputScheduler                        = _InputScheduler;
            ShredingerBlocksProceeder             = _ShredingerBlocksProceeder;
            SpringboardProceeder                  = _SpringboardProceeder;
        }

        public void PreInit()
        {
            foreach (var proceeder in GetInterfaceOfProceeders<IItemsProceeder>())
            {
                Data.MazeItemsProceedStarted += proceeder.Start;
                Data.MazeItemsProceedStopped += proceeder.Stop;
            }
            
            foreach (var item in GetInterfaceOfProceeders<IOnGameLoopUpdate>())
                Data.GameLoopUpdate += item.OnGameLoopUpdate;
            
            Data.MazeChanged                           += MazeOnMazeChanged;
            MazeRotation.RotationFinished              += MazeOnRotationFinished;
            Character.AliveOrDeath                            += OnCharacterAliveOrDeath;
            Character.CharacterMoveStarted             += CharacterOnMoveStarted;
            Character.CharacterMoveContinued           += CharacterOnMoveContinued;
            Character.CharacterMoveFinished            += CharacterOnFinishMove; 
            InputScheduler.MoveCommand                 += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand               += InputSchedulerOnRotateCommand;
            InputScheduler.OtherCommand                += InputSchedulerOnOtherCommand;
            PortalsProceeder.PortalEvent               += Character.OnPortal;
            SpringboardProceeder.SpringboardEvent      += Character.OnSpringboard;
            PathItemsProceeder.AllPathsProceededEvent  += AllPathsProceededEvent;
            
            Data.PreInitialized += () => PreInitialized?.Invoke();
            Data.PreInit();
        }
        
        public void Init()
        {
            Character.Initialized += () => Initialized?.Invoke();
            Character.Init();
        }
        
        #endregion

        #region nonpublic methods

        
        private void OnCharacterAliveOrDeath(bool _Alive)
        {
            if (!_Alive)
                LevelStaging.FinishLevel(false);
        }

        private void AllPathsProceededEvent() => LevelStaging.FinishLevel(true);

        private void MazeOnMazeChanged(MazeInfo _Info)
        {
            var proceeders = GetInterfaceOfProceeders<IOnMazeChanged>();
            foreach (var proceeder in proceeders)
                proceeder.OnMazeChanged(_Info);
        }

        private void MazeOnRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            InputScheduler.UnlockRotation(true);
            GravityItemsProceeder.OnMazeOrientationChanged();
        }
        
        private void CharacterOnMoveStarted(CharacterMovingEventArgs _Args)
        {
            GravityItemsProceeder.OnCharacterMoveStarted(_Args);
        }

        private void CharacterOnMoveContinued(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args) => InputScheduler.UnlockMovement(true);
        
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
                case EInputCommand.RotateClockwise:       
                    dir = MazeRotateDirection.Clockwise;        break;
                case EInputCommand.RotateCounterClockwise:
                    dir = MazeRotateDirection.CounterClockwise; break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            MazeRotation.Rotate(dir);
        }
        
        private void InputSchedulerOnOtherCommand(EInputCommand _Command)
        {
            if (_Command == EInputCommand.Restart)
                Data.RaiseMazeChanged();
        }
        
        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var result = new List<T>
            {
                Character                 as T,
                PathItemsProceeder        as T,
                MovingItemsProceeder      as T,
                GravityItemsProceeder     as T,
                TrapsReactProceeder       as T,
                TrapsIncreasingProceeder  as T,
                TurretsProceeder          as T,
                PortalsProceeder          as T,
                ShredingerBlocksProceeder as T,
                SpringboardProceeder      as T
            }.Where(_Proceeder => _Proceeder != null).ToList();
            return result;
        } 

        #endregion
    }
}
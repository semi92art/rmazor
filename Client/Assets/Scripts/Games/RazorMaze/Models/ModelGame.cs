using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IPreInit
    {
        IModelData                                Data { get; }
        IModelMazeRotation                        MazeRotation { get; }
        IPathItemsProceeder                       PathItemsProceeder { get; }
        ITrapsMovingProceeder                     TrapsMovingProceeder { get; }
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
        IEnumerable<IMazeItemProceedInfo>         GetAllProceedInfos();
    }
    
    public class ModelGame : IModelGame
    {
        #region api
        
        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler Initialized;
        
        public IModelData                         Data { get; }
        public IModelMazeRotation                 MazeRotation { get; }
        public IPathItemsProceeder                PathItemsProceeder { get; }
        public ITrapsMovingProceeder              TrapsMovingProceeder { get; }
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
            IModelData                            _Data,
            IModelMazeRotation                    _MazeRotation,
            IPathItemsProceeder                   _PathItemsProceeder,
            ITrapsMovingProceeder                 _TrapsMovingProceeder,
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
            TrapsMovingProceeder                  = _TrapsMovingProceeder;
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
            Data.MazeInfoSet                           += PathItemsProceeder.OnMazeInfoSet;
            MazeRotation.RotationFinishedInternal              += MazeOnRotationFinishedInternal;
            Character.AliveOrDeath                     += OnCharacterAliveOrDeath;
            Character.CharacterMoveStarted             += CharacterOnMoveStarted;
            Character.CharacterMoveContinued           += CharacterOnMoveContinued;
            Character.CharacterMoveFinished            += CharacterOnFinishMove; 
            InputScheduler.MoveCommand                 += InputSchedulerOnMoveCommand;
            InputScheduler.RotateCommand               += InputSchedulerOnRotateCommand;
            InputScheduler.OtherCommand                += InputSchedulerOnOtherCommand;
            PortalsProceeder.PortalEvent               += Character.OnPortal;
            SpringboardProceeder.SpringboardEvent      += Character.OnSpringboard;
            PathItemsProceeder.AllPathsProceededEvent  += AllPathsProceededEvent;
            LevelStaging.LevelStageChanged             += LevelStageChanged;
            
            var getProceedInfosItems = 
                RazorMazeUtils.GetInterfaceOfProceeders<IGetAllProceedInfos>(GetProceeders());
            foreach (var item in getProceedInfosItems)
                item.GetAllProceedInfos = GetAllProceedInfos;
            PreInitialized?.Invoke();
        }
        
        public void Init()
        {
            Character.Initialized += () => Initialized?.Invoke();
            Character.Init();
        }
        
        public IEnumerable<IMazeItemProceedInfo> GetAllProceedInfos()
        {
            var itemProceeders =
                RazorMazeUtils.GetInterfaceOfProceeders<IItemsProceeder>(GetProceeders());
            var result = itemProceeders
                .SelectMany(_P => _P.ProceedInfos.Values
                    .SelectMany(_V => _V));
            return result;
        }
        
        #endregion

        #region event methods
        
        private void OnCharacterAliveOrDeath(bool _Alive)
        {
            if (!_Alive)
                LevelStaging.FinishLevel();
        }

        private void AllPathsProceededEvent() => LevelStaging.FinishLevel();

        private void MazeOnRotationFinishedInternal(MazeRotateDirection _Direction, MazeOrientation _Orientation)
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
            var proceeders = 
                RazorMazeUtils.GetInterfaceOfProceeders<ICharacterMoveContinued>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args)
        {
            InputScheduler.UnlockMovement(true);
            ShredingerBlocksProceeder.OnCharacterMoveFinished(_Args);
        }

        private void LevelStageChanged(LevelStageArgs _Args)
        {
            var proceeders = 
                RazorMazeUtils.GetInterfaceOfProceeders<IOnLevelStageChanged>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnLevelStageChanged(_Args);
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void InputSchedulerOnMoveCommand(int _Command, object[] _Args)
        {
            EMazeMoveDirection dir = default;
            switch (_Command)
            {
                case (int)EInputCommand.MoveUp:    dir = EMazeMoveDirection.Up;    break;
                case (int)EInputCommand.MoveDown:  dir = EMazeMoveDirection.Down;  break;
                case (int)EInputCommand.MoveLeft:  dir = EMazeMoveDirection.Left;  break;
                case (int)EInputCommand.MoveRight: dir = EMazeMoveDirection.Right; break;
                case (int)EInputCommand.RotateClockwise:
                case (int)EInputCommand.RotateCounterClockwise:
                    break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            Character.Move(dir);
        }
        
        private void InputSchedulerOnRotateCommand(int _Command, object[] _Args)
        {
            MazeRotateDirection dir;
            switch (_Command)
            {
                case (int)EInputCommand.RotateClockwise:       
                    dir = MazeRotateDirection.Clockwise;        break;
                case (int)EInputCommand.RotateCounterClockwise:
                    dir = MazeRotateDirection.CounterClockwise; break;
                default: throw new SwitchCaseNotImplementedException(_Command);
            }
            MazeRotation.StartRotation(dir);
        }
        
        private void InputSchedulerOnOtherCommand(int _Command, object[] _Args)
        {
            switch (_Command)
            {
                case (int) EInputCommand.LoadLevel:
                    LevelStaging.LoadLevel(Data.Info, Data.LevelIndex);
                    break;
                case (int)EInputCommand.ReadyToContinueLevel:
                    LevelStaging.ReadyToContinueLevel();
                    break;
                case (int)EInputCommand.ContinueLevel:
                    LevelStaging.StartOrContinueLevel();
                    break;
                case (int)EInputCommand.FinishLevel:
                    LevelStaging.FinishLevel();
                    break;
                case (int)EInputCommand.PauseLevel:
                    LevelStaging.PauseLevel();
                    break;
                case (int)EInputCommand.UnloadLevel:
                    LevelStaging.UnloadLevel();
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Command);
            }
        }
        
        private List<object> GetProceeders()
        {
            var result = new List<object>
            {
                Character,
                PathItemsProceeder,
                TrapsMovingProceeder,
                GravityItemsProceeder,
                TrapsReactProceeder,
                TrapsIncreasingProceeder,
                TurretsProceeder,
                PortalsProceeder,
                ShredingerBlocksProceeder,
                SpringboardProceeder
            }.Where(_Proceeder => _Proceeder != null)
                .ToList();
            return result;
        } 

        #endregion
    }
}
using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models.InputSchedulers;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;
using Mono_Installers;
using UnityEngine.Events;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IOnLevelStageChanged
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
        IModelLevelStaging                        LevelStaging { get; }
        IInputScheduler                           InputScheduler { get; }
        IEnumerable<IMazeItemProceedInfo>         GetAllProceedInfos();
    }
    
    public class ModelGame : IModelGame
    {
        #region api
        
        public event UnityAction Initialized;
        
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
        public IModelLevelStaging                 LevelStaging { get; }
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
            IModelLevelStaging                    _Staging,
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
            LevelStaging                          = _Staging;
            InputScheduler                        = _InputScheduler;
            ShredingerBlocksProceeder             = _ShredingerBlocksProceeder;
            SpringboardProceeder                  = _SpringboardProceeder;
        }

        public void Init()
        {
            LevelStaging.LevelStageChanged             += OnLevelStageChanged;
            LevelStaging.LevelStageChanged             += MazeRotation.OnLevelStageChanged;
            MazeRotation.RotationFinishedInternal      += MazeOnRotationFinishedInternal;
            Character.CharacterMoveStarted             += CharacterOnMoveStarted;
            Character.CharacterMoveContinued           += CharacterOnMoveContinued;
            Character.CharacterMoveFinished            += CharacterOnFinishMove;
            PortalsProceeder.PortalEvent               += Character.OnPortal;
            SpringboardProceeder.SpringboardEvent      += Character.OnSpringboard;
            PathItemsProceeder.AllPathsProceededEvent  += AllPathsProceededEvent;

            var getProceedInfosItems = GetInterfaceOfProceeders<IGetAllProceedInfos>(GetProceeders());
            foreach (var item in getProceedInfosItems)
                item.GetAllProceedInfos = GetAllProceedInfos;

            Initialized?.Invoke();
        }

        public IEnumerable<IMazeItemProceedInfo> GetAllProceedInfos()
        {
            var itemProceeders = GetInterfaceOfProceeders<IItemsProceeder>(GetProceeders());
            var result = itemProceeders
                .SelectMany(_P => _P.ProceedInfos.Values
                    .SelectMany(_V => _V));
            return result;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<IOnLevelStageChanged>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnLevelStageChanged(_Args);
        }
        
        #endregion

        #region event methods
        
        private void AllPathsProceededEvent()
        {
            if (LevelMonoInstaller.Release)
                LevelStaging.FinishLevel();
        }

        private void MazeOnRotationFinishedInternal(MazeRotationEventArgs _Args)
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
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        private void CharacterOnFinishMove(CharacterMovingEventArgs _Args)
        {
            InputScheduler.UnlockMovement(true);
            ShredingerBlocksProceeder.OnCharacterMoveFinished(_Args);
        }

        #endregion
        
        #region nonpublic methods

        private List<object> GetProceeders()
        {
            var result = new List<object>
            {
                PathItemsProceeder,
                Character,
                TrapsMovingProceeder,
                GravityItemsProceeder,
                TrapsReactProceeder,
                TrapsIncreasingProceeder,
                TurretsProceeder,
                PortalsProceeder,
                ShredingerBlocksProceeder,
                SpringboardProceeder,
                InputScheduler,
                LevelStaging,
                MazeRotation
            }.Where(_Proceeder => _Proceeder != null)
                .ToList();
            return result;
        } 
        
        private static List<T> GetInterfaceOfProceeders<T>(IEnumerable<object> _Proceeders) where T : class
        {
            return _Proceeders.Where(_Proceeder => _Proceeder is T).Cast<T>().ToList();
        }

        #endregion
    }
}
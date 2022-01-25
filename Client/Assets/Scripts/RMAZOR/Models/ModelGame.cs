using System.Collections.Generic;
using System.Linq;
using Common;
using RMAZOR.Models.InputSchedulers;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;
using UnityEngine.Events;

namespace RMAZOR.Models
{
    public interface IModelGame : IInit, IOnLevelStageChanged
    {
        ModelSettings              Settings                  { get; }
        IModelData                 Data                      { get; }
        IModelMazeRotation         MazeRotation              { get; }
        IPathItemsProceeder        PathItemsProceeder        { get; }
        ITrapsMovingProceeder      TrapsMovingProceeder      { get; }
        IGravityItemsProceeder     GravityItemsProceeder     { get; }
        ITrapsReactProceeder       TrapsReactProceeder       { get; }
        ITrapsIncreasingProceeder  TrapsIncreasingProceeder  { get; }
        ITurretsProceeder          TurretsProceeder          { get; }
        IPortalsProceeder          PortalsProceeder          { get; }
        IShredingerBlocksProceeder ShredingerBlocksProceeder { get; }
        ISpringboardProceeder      SpringboardProceeder      { get; }
        IModelCharacter            Character                 { get; }
        IModelLevelStaging         LevelStaging              { get; }
        IInputScheduler            InputScheduler            { get; }
        IMazeItemProceedInfo[] GetAllProceedInfos();
    }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ModelGame : IModelGame
    {
        #region nonpublic members

        private IMazeItemProceedInfo[] m_AllProceedInfosCached;
        private List<object>           m_Proceeders;

        #endregion

        #region inject

        public ModelSettings              Settings                  { get; }
        public IModelData                 Data                      { get; }
        public IModelMazeRotation         MazeRotation              { get; }
        public IPathItemsProceeder        PathItemsProceeder        { get; }
        public ITrapsMovingProceeder      TrapsMovingProceeder      { get; }
        public IGravityItemsProceeder     GravityItemsProceeder     { get; }
        public ITrapsReactProceeder       TrapsReactProceeder       { get; }
        public ITrapsIncreasingProceeder  TrapsIncreasingProceeder  { get; }
        public ITurretsProceeder          TurretsProceeder          { get; }
        public IPortalsProceeder          PortalsProceeder          { get; }
        public IShredingerBlocksProceeder ShredingerBlocksProceeder { get; }
        public ISpringboardProceeder      SpringboardProceeder      { get; }
        public IModelCharacter            Character                 { get; }
        public IModelLevelStaging         LevelStaging              { get; }
        public IInputScheduler            InputScheduler            { get; }

        public ModelGame(
            ModelSettings                         _Settings,
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
            Settings                              = _Settings;
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

        #endregion
        
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;

        public void Init()
        {
            m_Proceeders = new List<object>
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
            };
            LevelStaging.LevelStageChanged                 += OnLevelStageChanged;
            MazeRotation.RotationStarted                   += OnMazeRotationStarted;
            MazeRotation.RotationFinished                  += OnMazeRotationFinished;
            Character.CharacterMoveStarted                 += OnCharacterMoveStarted;
            Character.CharacterMoveContinued               += OnCharacterMoveContinued;
            Character.CharacterMoveFinished                += OnCharacterMoveFinished;
            PortalsProceeder.PortalEvent                   += Character.OnPortal;
            SpringboardProceeder.SpringboardEvent          += Character.OnSpringboard;
            ShredingerBlocksProceeder.ShredingerBlockEvent += GravityItemsProceeder.OnShredingerBlockEvent;
            var getProceedInfosItems = GetInterfaceOfProceeders<IGetAllProceedInfos>(m_Proceeders);
            foreach (var item in getProceedInfosItems)
                item.GetAllProceedInfos = GetAllProceedInfos;
            Character.GetStartPosition = () => PathItemsProceeder.PathProceeds.First().Key;
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public IMazeItemProceedInfo[] GetAllProceedInfos()
        {
            if (m_AllProceedInfosCached != null)
                return m_AllProceedInfosCached;
            var itemProceeders =
                GetInterfaceOfProceeders<IItemsProceeder>(m_Proceeders);
            var result = itemProceeders
                .SelectMany(_P => _P.ProceedInfos)
                .ToArray();
            m_AllProceedInfosCached = result;
            return result;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                m_AllProceedInfosCached = null;
            var proceeders =
                GetInterfaceOfProceeders<IOnLevelStageChanged>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnLevelStageChanged(_Args);
        }
        
        #endregion

        #region event methods
        
        private void OnMazeRotationStarted(MazeRotationEventArgs _Args)
        {
            if (LevelStaging.LevelStage == ELevelStage.ReadyToStart 
                && LevelStaging.PrevLevelStage == ELevelStage.Loaded)
            {
                LevelStaging.StartOrContinueLevel();
            }
            InputScheduler.LockRotation(true);
            InputScheduler.LockMovement(true);
        }
        
        private void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            InputScheduler.LockRotation(false);
            InputScheduler.LockMovement(false);
            GravityItemsProceeder.OnMazeOrientationChanged();
        }
        
        private void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            var proceeders = 
                GetInterfaceOfProceeders<ICharacterMoveStarted>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnCharacterMoveStarted(_Args);
            InputScheduler.LockMovement(true);
            InputScheduler.LockRotation(true);
        }

        private void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var proceeders =
                GetInterfaceOfProceeders<ICharacterMoveContinued>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnCharacterMoveContinued(_Args);
        }
        
        private void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var proceeders =
                GetInterfaceOfProceeders<ICharacterMoveFinished>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnCharacterMoveFinished(_Args);
            InputScheduler.LockMovement(false);
            InputScheduler.LockRotation(false);
        }

        #endregion
        
        #region nonpublic methods
        
        private static List<T> GetInterfaceOfProceeders<T>(IReadOnlyList<object> _Proceeders) where T : class
        {
            var result = new List<T>();
            for (int i = 0; i < _Proceeders.Count; i++)
            {
                if (_Proceeders[i] is T tVal)
                    result.Add(tVal);
            }
            return result;
        }

        #endregion
    }
}
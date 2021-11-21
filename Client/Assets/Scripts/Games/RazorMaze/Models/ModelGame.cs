using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models.InputSchedulers;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;
using UnityEngine.Events;

namespace Games.RazorMaze.Models
{
    public interface IModelGame : IInit, IOnLevelStageChanged
    {
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
        List<IMazeItemProceedInfo> GetAllProceedInfos();
    }
    
    public class ModelGame : IModelGame
    {
        #region nonpublic members

        private List<IMazeItemProceedInfo> m_AllProceedInfosCached;
        private List<object>               m_Proceeders;

        #endregion
        
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
            MazeRotation.RotationFinishedInternal          += MazeOnRotationFinishedInternal;
            Character.CharacterMoveStarted                 += CharacterOnMoveStarted;
            Character.CharacterMoveContinued               += CharacterOnMoveContinued;
            Character.CharacterMoveFinished                += CharacterOnMoveFinished;
            PortalsProceeder.PortalEvent                   += Character.OnPortal;
            SpringboardProceeder.SpringboardEvent          += Character.OnSpringboard;
            ShredingerBlocksProceeder.ShredingerBlockEvent += GravityItemsProceeder.OnShredingerBlockEvent;

            var getProceedInfosItems = GetInterfaceOfProceeders<IGetAllProceedInfos>(m_Proceeders);
            foreach (var item in getProceedInfosItems)
                item.GetAllProceedInfos = GetAllProceedInfos;

            Initialized?.Invoke();
        }
        
        public List<IMazeItemProceedInfo> GetAllProceedInfos()
        {
            if (m_AllProceedInfosCached != null)
                return m_AllProceedInfosCached;
            var itemProceeders =
                GetInterfaceOfProceeders<IItemsProceeder>(m_Proceeders);
            var result = itemProceeders
                .SelectMany(_P => _P.ProceedInfos)
                .ToList();
            m_AllProceedInfosCached = result;
            return result;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Unloaded)
                m_AllProceedInfosCached = null;
            var proceeders =
                GetInterfaceOfProceeders<IOnLevelStageChanged>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnLevelStageChanged(_Args);
        }
        
        #endregion

        #region event methods
        
        private void MazeOnRotationFinishedInternal(MazeRotationEventArgs _Args)
        {
            InputScheduler.UnlockRotation(true);
            GravityItemsProceeder.OnMazeOrientationChanged();
        }
        
        private void CharacterOnMoveStarted(CharacterMovingEventArgs _Args)
        {
            var proceeders = 
                GetInterfaceOfProceeders<ICharacterMoveStarted>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnCharacterMoveStarted(_Args);
        }

        private void CharacterOnMoveContinued(CharacterMovingEventArgs _Args)
        {
            var proceeders =
                GetInterfaceOfProceeders<ICharacterMoveContinued>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnCharacterMoveContinued(_Args);
        }
        
        private void CharacterOnMoveFinished(CharacterMovingEventArgs _Args)
        {
            var proceeders =
                GetInterfaceOfProceeders<ICharacterMoveFinished>(m_Proceeders);
            for (int i = 0; i < proceeders.Count; i++)
                proceeders[i].OnCharacterMoveFinished(_Args);
            InputScheduler.UnlockMovement(true);
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
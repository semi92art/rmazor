using System;
using System.Linq;
using Common;
using Common.Helpers;
using RMAZOR.Models.InputSchedulers;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;

namespace RMAZOR.Models
{
    public interface IModelGame : IInit, IOnLevelStageChanged
    {
        ModelSettings          Settings           { get; }
        IModelData             Data               { get; }
        IModelMazeRotation     MazeRotation       { get; }
        IPathItemsProceeder    PathItemsProceeder { get; }
        IModelItemsProceedersSet    ModelItemsProceedersSet { get; }
        IModelCharacter        Character          { get; }
        IModelLevelStaging     LevelStaging       { get; }
        IInputScheduler        InputScheduler     { get; }
        IMazeItemProceedInfo[] GetAllProceedInfos();
    }
    
    public class ModelGame : InitBase, IModelGame
    {
        #region nonpublic members

        private IMazeItemProceedInfo[] m_AllProceedInfosCached;
        private object[]               m_ProceedersCached;

        #endregion

        #region inject

        public ModelSettings            Settings                { get; }
        public IModelData               Data                    { get; }
        public IModelMazeRotation       MazeRotation            { get; }
        public IPathItemsProceeder      PathItemsProceeder      { get; }
        public IModelItemsProceedersSet ModelItemsProceedersSet { get; }
        public IModelCharacter          Character               { get; }
        public IModelLevelStaging       LevelStaging            { get; }
        public IInputScheduler          InputScheduler          { get; }

        private ModelGame(
            ModelSettings            _Settings,
            IModelData               _Data,
            IModelMazeRotation       _MazeRotation,
            IPathItemsProceeder      _PathItemsProceeder,
            IModelItemsProceedersSet _ModelItemsProceedersSet,
            IModelCharacter          _CharacterModel,
            IModelLevelStaging       _Staging,
            IInputScheduler          _InputScheduler)
        {
            Settings                 = _Settings;
            Data                     = _Data;
            MazeRotation             = _MazeRotation;
            PathItemsProceeder       = _PathItemsProceeder;
            Character                = _CharacterModel;
            LevelStaging             = _Staging;
            InputScheduler           = _InputScheduler;
            ModelItemsProceedersSet  = _ModelItemsProceedersSet;
        }

        #endregion
        
        #region api
        
        public override void Init()
        {
            m_ProceedersCached = new object[]
            {
                PathItemsProceeder,
                Character,
                ModelItemsProceedersSet,
                InputScheduler,
                LevelStaging,
                MazeRotation
            };
            LevelStaging.LevelStageChanged   += OnLevelStageChanged;
            MazeRotation.RotationStarted     += OnMazeRotationStarted;
            MazeRotation.RotationFinished    += OnMazeRotationFinished;
            Character.CharacterMoveStarted   += OnCharacterMoveStarted;
            Character.CharacterMoveContinued += OnCharacterMoveContinued;
            Character.CharacterMoveFinished  += OnCharacterMoveFinished;
            ModelItemsProceedersSet.PortalsProceeder.PortalEvent          += Character.OnPortal;
            ModelItemsProceedersSet.SpringboardProceeder.SpringboardEvent += Character.OnSpringboard;
            ModelItemsProceedersSet.ShredingerBlocksProceeder.ShredingerBlockEvent += 
                ModelItemsProceedersSet.GravityItemsProceeder.OnShredingerBlockEvent;
            var notNullProceeders = GetInterfaceOfProceeders<IGetAllProceedInfos>()
                .Where(_P => _P != null);
            foreach (var item in notNullProceeders)
                item.GetAllProceedInfos = GetAllProceedInfos;
            Character.GetStartPosition = () => PathItemsProceeder.PathProceeds.First().Key;
            base.Init();
        }
        
        public IMazeItemProceedInfo[] GetAllProceedInfos()
        {
            if (m_AllProceedInfosCached != null)
                return m_AllProceedInfosCached;
            var itemProceeders =
                ModelItemsProceedersSet.GetProceeders();
            var result = itemProceeders
                .Where(_P => _P != null)
                .SelectMany(_P => _P.ProceedInfos)
                .ToArray();
            m_AllProceedInfosCached = result;
            return result;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.Loaded)
                m_AllProceedInfosCached = null;
            var proceeders =
                GetInterfaceOfProceeders<IOnLevelStageChanged>();
            for (int i = 0; i < proceeders.Length; i++)
                proceeders[i]?.OnLevelStageChanged(_Args);
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
            ModelItemsProceedersSet.GravityItemsProceeder.OnMazeOrientationChanged();
        }
        
        private void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            var proceeders = 
                GetInterfaceOfProceeders<ICharacterMoveStarted>();
            for (int i = 0; i < proceeders.Length; i++)
                proceeders[i]?.OnCharacterMoveStarted(_Args);
            InputScheduler.LockMovement(true);
            InputScheduler.LockRotation(true);
        }

        private void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var proceeders =
                GetInterfaceOfProceeders<ICharacterMoveContinued>();
            for (int i = 0; i < proceeders.Length; i++)
                proceeders[i]?.OnCharacterMoveContinued(_Args);
        }
        
        private void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var proceeders =
                GetInterfaceOfProceeders<ICharacterMoveFinished>();
            for (int i = 0; i < proceeders.Length; i++)
                proceeders[i]?.OnCharacterMoveFinished(_Args);
            InputScheduler.LockMovement(false);
            InputScheduler.LockRotation(false);
        }

        #endregion
        
        #region nonpublic methods
        
        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }

        #endregion
    }
}
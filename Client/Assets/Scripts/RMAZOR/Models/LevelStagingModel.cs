using Common.Helpers;
using Common.Ticker;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models
{
    public enum ELevelStage
    {
        Loaded,
        ReadyToStart,
        StartedOrContinued,
        Paused,
        Finished,
        ReadyToUnloadLevel,
        Unloaded,
        CharacterKilled
    }

    public class LevelStageArgs : EventArgsEx
    {
        public long        LevelIndex       { get; }
        public ELevelStage LevelStage       { get; }
        public ELevelStage PreviousStage    { get; }
        public ELevelStage PrePreviousStage { get; }

        public LevelStageArgs(
            long        _LevelIndex,
            ELevelStage _LevelLevelStage,
            ELevelStage _PreviousStage,
            ELevelStage _PrePreviousStage)
        {
            LevelIndex       = _LevelIndex;
            LevelStage       = _LevelLevelStage;
            PreviousStage    = _PreviousStage;
            PrePreviousStage = _PrePreviousStage;
        }
    }
    public delegate void LevelStageHandler(LevelStageArgs _Args);


    public interface IModelLevelStaging
    {
        long                    LevelIndex         { get; set; }
        float                   LevelTime          { get; }
        int                     DiesCount          { get; }
        ELevelStage             LevelStage         { get; }
        ELevelStage             PrevLevelStage     { get; }
        ELevelStage             PrevPrevLevelStage { get; }
        event LevelStageHandler LevelStageChanged;
        void                    LoadLevel(MazeInfo _Info, long _LevelIndex, object[] _Args = null);
        void                    ReadyToStartLevel();
        void                    StartOrContinueLevel();
        void                    PauseLevel();
        void                    UnPauseLevel();
        void                    FinishLevel();
        void                    KillCharacter();
        void                    ReadyToUnloadLevel(object[] _Args = null);
        void                    UnloadLevel(object[]        _Args = null);
    }

    public class ModelLevelStaging : InitBase, IModelLevelStaging, IUpdateTick
    {
        #region nonpublic members

        private bool m_DoUpdateLevelTime;
        
        #endregion
    
        #region inject
    
        private IModelData       Data       { get; }
        private IModelGameTicker GameTicker { get; }

        public ModelLevelStaging(IModelData _Data, IModelGameTicker _GameTicker)
        {
            Data       = _Data;
            GameTicker = _GameTicker;
        }
    
        #endregion
    
        #region api

        public long                    LevelIndex         { get; set; }
        public float                   LevelTime          { get; private set; }
        public int                     DiesCount          { get; private set; }
        public ELevelStage             LevelStage         { get; private set; } = ELevelStage.Unloaded;
        public ELevelStage             PrevLevelStage     { get; private set; } = ELevelStage.Unloaded;
        public ELevelStage             PrevPrevLevelStage { get; private set; } = ELevelStage.Unloaded;
        public event LevelStageHandler LevelStageChanged;

        public override void Init()
        {
            GameTicker.Register(this);
            base.Init();
        }

        public void UpdateTick()
        {
            if (m_DoUpdateLevelTime)
                LevelTime += GameTicker.DeltaTime;
        }
    
        public virtual void LoadLevel(MazeInfo _Info, long _LevelIndex, object[] _Args = null)
        {
            (Data.Info, LevelIndex) = (_Info, _LevelIndex);
            InvokeLevelStageChanged(ELevelStage.Loaded, _Args);
        }

        public void ReadyToStartLevel()
        {
            InvokeLevelStageChanged(ELevelStage.ReadyToStart);
        }

        public void StartOrContinueLevel()
        {
            InvokeLevelStageChanged(ELevelStage.StartedOrContinued);
        }

        public void PauseLevel()
        {
            InvokeLevelStageChanged(ELevelStage.Paused);
        }

        public void UnPauseLevel()
        {
            InvokeLevelStageChanged(PrevLevelStage);
        }

        public void FinishLevel()
        {
            InvokeLevelStageChanged(ELevelStage.Finished);
        }

        public void KillCharacter()
        {
            InvokeLevelStageChanged(ELevelStage.CharacterKilled);
        }

        public void ReadyToUnloadLevel(object[] _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.ReadyToUnloadLevel, _Args);
        }

        public void UnloadLevel(object[] _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.Unloaded, _Args);
        }

        #endregion

        #region nonpublic methods

        private void InvokeLevelStageChanged(ELevelStage _Stage, object[] _Args = null)
        {
            PrevPrevLevelStage = PrevLevelStage;
            PrevLevelStage = LevelStage;
            m_DoUpdateLevelTime = _Stage == ELevelStage.StartedOrContinued;
            if (_Stage == ELevelStage.ReadyToStart && PrevLevelStage != ELevelStage.Paused)
                LevelTime = 0f;
            if (_Stage == ELevelStage.CharacterKilled)
                DiesCount++;
            else if (_Stage == ELevelStage.Loaded)
                DiesCount = 0;
            LevelStage = _Stage;
            var args = new LevelStageArgs(
                LevelIndex,
                LevelStage,
                PrevLevelStage,
                PrevPrevLevelStage) {Args = _Args};
            LevelStageChanged?.Invoke(args);
        }
    
        #endregion
    }
}
using System.Collections.Generic;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Models
{
    public class LevelInfoArgs
    {
        public long   LevelIndex { get; set; }
        public string GameMode   { get; set; }
        public string LevelType  { get; set; }
        
        public Dictionary<string, object> Arguments  { get; } = new Dictionary<string, object>();
    }
    
    public enum ELevelStage
    {
        None,
        Loaded,
        ReadyToStart,
        StartedOrContinued,
        Paused,
        Finished,
        ReadyToUnloadLevel,
        Unloaded,
        CharacterKilled
    }

    public class LevelStageArgs : System.EventArgs
    {
        public long                       LevelIndex          { get; }
        public ELevelStage                LevelStage          { get; }
        public ELevelStage                PreviousStage       { get; }
        public ELevelStage                PrePreviousStage    { get; }
        public ELevelStage                PrePrePreviousStage { get; }
        public float                      LevelTime           { get; }
        public Dictionary<string, object> Arguments           { get; }

        public LevelStageArgs(
            long                       _LevelIndex,
            ELevelStage                _LevelLevelStage,
            ELevelStage                _PreviousStage,
            ELevelStage                _PrePreviousStage,
            ELevelStage                _PrePrePreviousStage,
            float                      _LevelTime,
            Dictionary<string, object> _Args)
        {
            LevelIndex          = _LevelIndex;
            LevelStage          = _LevelLevelStage;
            PreviousStage       = _PreviousStage;
            PrePreviousStage    = _PrePreviousStage;
            PrePrePreviousStage = _PrePrePreviousStage;
            LevelTime = _LevelTime;
            Arguments                = _Args ?? new Dictionary<string, object>();
        }
    }
    public delegate void LevelStageHandler(LevelStageArgs _Args);


    public interface IModelLevelStaging
    {
        event LevelStageHandler    LevelStageChanged;
        long                       LevelIndex     { get; }
        float                      LevelTime      { get; }
        int                        DiesCount      { get; }
        ELevelStage                LevelStage     { get; }
        ELevelStage                PrevLevelStage { get; }
        Dictionary<string, object> Arguments      { get; }

        void LoadLevel(MazeInfo _Info, long _LevelIndex, Dictionary<string, object> _Args = null);
        
        void ReadyToStartLevel(Dictionary<string, object>    _Args = null);
        void StartOrContinueLevel(Dictionary<string, object> _Args = null);
        void PauseLevel(Dictionary<string, object>           _Args = null);
        void UnPauseLevel(Dictionary<string, object>         _Args = null);
        void FinishLevel(Dictionary<string, object>          _Args = null);
        void KillCharacter(Dictionary<string, object>        _Args = null);
        void ReadyToUnloadLevel(Dictionary<string, object>   _Args = null);
        void UnloadLevel(Dictionary<string, object>          _Args = null);
        void ExitLevelStaging(Dictionary<string, object>     _Args = null);

        LevelStageArgs GetCurrentLevelStageArguments();
    }

    public class ModelLevelStaging : InitBase, IModelLevelStaging, IUpdateTick
    {
        #region nonpublic members

        private bool        m_DoUpdateLevelTime;
        private ELevelStage PrevPrevLevelStage { get; set; }     = ELevelStage.None;
        private ELevelStage PrevPrevPrevLevelStage { get; set; } = ELevelStage.None;
        
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

        public long                       LevelIndex     { get; private set; }
        public float                      LevelTime      { get; private set; }
        public int                        DiesCount      { get; private set; }
        public ELevelStage                LevelStage     { get; private set; } = ELevelStage.None;
        public ELevelStage                PrevLevelStage { get; private set; } = ELevelStage.None;
        public Dictionary<string, object> Arguments      { get; private set; }

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
    
        public virtual void LoadLevel(MazeInfo _Info, long _LevelIndex, Dictionary<string, object> _Args = null)
        {
            (Data.Info, LevelIndex) = (_Info, _LevelIndex);
            InvokeLevelStageChanged(ELevelStage.Loaded, _Args);
        }

        public void ReadyToStartLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.ReadyToStart, _Args);
        }

        public void StartOrContinueLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.StartedOrContinued, _Args);
        }

        public void PauseLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.Paused, _Args);
        }

        public void UnPauseLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(PrevLevelStage, _Args);
        }

        public void FinishLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.Finished, _Args);
        }

        public void KillCharacter(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.CharacterKilled, _Args);
        }

        public void ReadyToUnloadLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.ReadyToUnloadLevel, _Args);
        }

        public void UnloadLevel(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.Unloaded, _Args);
        }

        public void ExitLevelStaging(Dictionary<string, object> _Args = null)
        {
            InvokeLevelStageChanged(ELevelStage.None, _Args);
        }

        public LevelStageArgs GetCurrentLevelStageArguments()
        {
            return new LevelStageArgs(
                LevelIndex,
                LevelStage,
                PrevLevelStage,
                PrevPrevLevelStage,
                PrevPrevPrevLevelStage,
                LevelTime,
                Arguments);
        }

        #endregion

        #region nonpublic methods

        private void InvokeLevelStageChanged(ELevelStage _Stage, Dictionary<string, object> _Args)
        {
            if (_Stage == LevelStage && _Stage != ELevelStage.None)
                return;
            if (_Args != null)
                Arguments = _Args;
            PrevPrevPrevLevelStage = PrevPrevLevelStage;
            PrevPrevLevelStage     = PrevLevelStage;
            PrevLevelStage         = LevelStage;
            m_DoUpdateLevelTime = _Stage == ELevelStage.StartedOrContinued;
            switch (_Stage)
            {
                case ELevelStage.ReadyToStart when PrevLevelStage != ELevelStage.Paused:
                    LevelTime = 0f;
                    break;
                case ELevelStage.CharacterKilled:
                    DiesCount++;
                    break;
                case ELevelStage.Loaded:
                    DiesCount = 0;
                    break;
            }
            LevelStage = _Stage;
            var args = GetCurrentLevelStageArguments();
            LevelStageChanged?.Invoke(args);
        }
    
        #endregion
    }
}
﻿using System;
using Common;
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
        public ELevelStage Stage            { get; }
        public ELevelStage PreviousStage    { get; }
        public ELevelStage PrePreviousStage { get; }

        public LevelStageArgs(
            long        _LevelIndex,
            ELevelStage _LevelStage,
            ELevelStage _PreviousStage,
            ELevelStage _PrePreviousStage)
        {
            LevelIndex = _LevelIndex;
            Stage = _LevelStage;
            PreviousStage = _PreviousStage;
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
        void                    LoadLevel(MazeInfo _Info, long _LevelIndex);
        void                    ReadyToStartLevel();
        void                    StartOrContinueLevel();
        void                    PauseLevel();
        void                    UnPauseLevel();
        void                    FinishLevel();
        void                    KillCharacter();
        void                    ReadyToUnloadLevel();
        void                    UnloadLevel();
    }

    public class ModelLevelStaging : InitBase, IModelLevelStaging,IUpdateTick
    {
        #region nonpublic members

        private bool m_DoUpdateLevelTime;

        #endregion
    
        #region inject
    
        private IModelData       Data       { get; }
        private IModelGameTicker GameTicker { get; }

        public ModelLevelStaging(IModelData _Data, IModelGameTicker _GameTicker)
        {
            Data = _Data;
            GameTicker = _GameTicker;
            _GameTicker.Register(this);
        }
    
        #endregion
    
        #region api

        public long                     LevelIndex         { get; set; }
        public float                   LevelTime          { get; private set; }
        public int                     DiesCount          { get; private set; }
        public ELevelStage             LevelStage         { get; private set; } = ELevelStage.Unloaded;
        public ELevelStage             PrevLevelStage     { get; private set; } = ELevelStage.Unloaded;
        public ELevelStage             PrevPrevLevelStage { get; private set; } = ELevelStage.Unloaded;
        public event LevelStageHandler LevelStageChanged;

        public void UpdateTick()
        {
            if (m_DoUpdateLevelTime)
                LevelTime += GameTicker.DeltaTime;
        }
    
        public virtual void LoadLevel(MazeInfo _Info, long _LevelIndex)
        {
            (Data.Info, LevelIndex) = (_Info, _LevelIndex);
            InvokeLevelStageChanged(ELevelStage.Loaded);
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
            Dbg.Log(nameof(KillCharacter));
            InvokeLevelStageChanged(ELevelStage.CharacterKilled);
        }

        public void ReadyToUnloadLevel()
        {
            InvokeLevelStageChanged(ELevelStage.ReadyToUnloadLevel);
        }

        public void UnloadLevel()
        {
            InvokeLevelStageChanged(ELevelStage.Unloaded);
        }

        #endregion

        #region nonpublic methods

        private void InvokeLevelStageChanged(ELevelStage _Stage)
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
            LevelStageChanged?.Invoke(new LevelStageArgs(
                                          LevelIndex,
                                          LevelStage,
                                          PrevLevelStage,
                                          PrevPrevLevelStage));
        }
    
        #endregion
    }
}
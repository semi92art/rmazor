using System;
using Games.RazorMaze.Models;
using Ticker;
using UnityEngine;
using UnityEngine.Events;

public enum ELevelStage
{
    Loaded,
    ReadyToStartOrContinue,
    StartedOrContinued,
    Paused,
    Finished,
    ReadyToUnloadLevel,
    Unloaded,
    CharacterKilled
}

public class LevelStageArgs : EventArgs
{
    public int LevelIndex { get; }
    public ELevelStage Stage { get; }
    public ELevelStage PreviousStage { get; }

    public LevelStageArgs(int _LevelIndex, ELevelStage _LevelStage, ELevelStage _PreviousStage) =>
        (LevelIndex, Stage, PreviousStage) = (_LevelIndex, _LevelStage, _PreviousStage);
}
public delegate void LevelStageHandler(LevelStageArgs _Args);


public interface IModelLevelStaging
{
    float LevelTime { get; }
    int DiesCount { get; }
    ELevelStage LevelStage { get; }
    event LevelStageHandler LevelStageChanged;
    void LoadLevel(MazeInfo _Info, int _LevelIndex);
    void ReadyToStartOrContinueLevel();
    void StartOrContinueLevel();
    void PauseLevel();
    void FinishLevel();
    void KillCharacter();
    void ReadyToUnloadLevel();
    void UnloadLevel();
}

public class ModelLevelStaging : IModelLevelStaging, IInit, IUpdateTick
{
    #region nonpublic members

    private bool m_DoUpdateLevelTime;

    #endregion
    
    #region inject
    
    private IModelData Data { get; }
    private IGameTicker GameTicker { get; }

    public ModelLevelStaging(IModelData _Data, IGameTicker _GameTicker)
    {
        Data = _Data;
        GameTicker = _GameTicker;
        _GameTicker.Register(this);
    }
    
    #endregion
    
    #region api

    public float LevelTime { get; private set; }
    public int DiesCount { get; private set; }
    public ELevelStage LevelStage { get; private set; } = ELevelStage.Unloaded;
    public event LevelStageHandler LevelStageChanged;
    public event UnityAction Initialized;
    
    public void Init()
    {
        Initialized?.Invoke();
    }
    
    public void UpdateTick()
    {
        if (m_DoUpdateLevelTime)
            LevelTime += Time.deltaTime;
    }
    
    public virtual void LoadLevel(MazeInfo _Info, int _LevelIndex)
    {
        (Data.Info, Data.LevelIndex) = (_Info, _LevelIndex);
        InvokeLevelStageChanged(ELevelStage.Loaded);
    }

    public void ReadyToStartOrContinueLevel()
    {
        InvokeLevelStageChanged(ELevelStage.ReadyToStartOrContinue);
    }

    public void StartOrContinueLevel()
    {
        InvokeLevelStageChanged(ELevelStage.StartedOrContinued);
    }

    public void PauseLevel()
    {
        InvokeLevelStageChanged(ELevelStage.Paused);
    }

    public void FinishLevel()
    {
        InvokeLevelStageChanged(ELevelStage.Finished);
    }

    public void KillCharacter()
    {
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
        var prevStage = LevelStage;
        m_DoUpdateLevelTime = _Stage == ELevelStage.StartedOrContinued;
        if (_Stage == ELevelStage.ReadyToStartOrContinue && prevStage != ELevelStage.Paused)
            LevelTime = 0f;
        if (_Stage == ELevelStage.CharacterKilled)
            DiesCount++;
        else if (_Stage == ELevelStage.Loaded)
            DiesCount = 0;
            
        LevelStage = _Stage;
        LevelStageChanged?.Invoke(new LevelStageArgs(Data.LevelIndex, LevelStage, prevStage));
    }
    
    #endregion
}
using System;
using Games.RazorMaze.Models;
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

public class ModelLevelStaging : IModelLevelStaging, IInit
{
    #region inject
    
    private IModelData Data { get; }

    public ModelLevelStaging(IModelData _Data)
    {
        Data = _Data;
    }
    
    #endregion
    
    #region api

    public ELevelStage LevelStage { get; private set; } = ELevelStage.Unloaded;
    public event LevelStageHandler LevelStageChanged;
    public event UnityAction Initialized;
    
    public void Init()
    {
        Initialized?.Invoke();
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
        LevelStage = _Stage;
        LevelStageChanged?.Invoke(new LevelStageArgs(Data.LevelIndex, LevelStage, prevStage));
    }
    
    #endregion
}
using System;
using Games.RazorMaze.Models;

public enum ELevelStage
{
    Loaded,
    ReadyToStartOrContinue,
    StartedOrContinued,
    Paused,
    Finished,
    Unloaded
}

public class LevelStageArgs : EventArgs
{
    public int LevelIndex { get; }
    public ELevelStage Stage { get; }

    public LevelStageArgs(int _LevelIndex, ELevelStage _LevelStage) =>
        (LevelIndex, Stage) = (_LevelIndex, _LevelStage);
}
public delegate void LevelStageHandler(LevelStageArgs _Args);


public interface ILevelStagingModel
{
    ELevelStage LevelStage { get; }
    event LevelStageHandler LevelStageChanged;
    void LoadLevel(MazeInfo _Info, int _LevelIndex);
    void PauseLevel();
    void ReadyToContinueLevel();
    void StartOrContinueLevel();
    void FinishLevel();
    void UnloadLevel();
}

public class LevelStagingModel : ILevelStagingModel
{
    #region inject
    
    private IModelData Data { get; }

    public LevelStagingModel(IModelData _Data)
    {
        Data = _Data;
    }
    
    #endregion
    
    #region api

    public ELevelStage LevelStage { get; private set; }
    public event LevelStageHandler LevelStageChanged;
    
    public virtual void LoadLevel(MazeInfo _Info, int _LevelIndex)
    {
        (Data.Info, Data.LevelIndex) = (_Info, _LevelIndex);
        LevelStage = ELevelStage.Loaded;
        InvokeLevelStageChanged();
    }

    public void PauseLevel()
    {
        LevelStage = ELevelStage.Paused;
        InvokeLevelStageChanged();
    }

    public void ReadyToContinueLevel()
    {
        LevelStage = ELevelStage.ReadyToStartOrContinue;
        InvokeLevelStageChanged();
    }

    public void StartOrContinueLevel()
    {
        LevelStage = ELevelStage.StartedOrContinued;
        InvokeLevelStageChanged();
    }

    public void FinishLevel()
    {
        LevelStage = ELevelStage.Finished;
        InvokeLevelStageChanged();
    }

    public void UnloadLevel()
    {
        LevelStage = ELevelStage.Unloaded;
        InvokeLevelStageChanged();
    }

    #endregion

    #region nonpublic methods

    private void InvokeLevelStageChanged()
    {
        LevelStageChanged?.Invoke(new LevelStageArgs(Data.LevelIndex, LevelStage));
    }

    #endregion
}
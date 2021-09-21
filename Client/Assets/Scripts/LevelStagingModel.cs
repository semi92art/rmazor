using System;
using Games.RazorMaze.Models;

public enum ELevelStage
{
    Loaded,
    Started,
    Paused,
    ReadyToContinue,
    Continued,
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
    void StartLevel();
    void PauseLevel();
    void ReadyToContinueLevel();
    void ContinueLevel();
    void FinishLevel();
    void UnloadLevel();
}

public class LevelStagingModel : ILevelStagingModel
{
    #region inject
    
    private IModelMazeData Data { get; }

    public LevelStagingModel(IModelMazeData _Data)
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

    public virtual void StartLevel()
    {
        LevelStage = ELevelStage.Started;
        InvokeLevelStageChanged();
    }

    public void PauseLevel()
    {
        LevelStage = ELevelStage.Paused;
        InvokeLevelStageChanged();
    }

    public void ReadyToContinueLevel()
    {
        LevelStage = ELevelStage.ReadyToContinue;
        InvokeLevelStageChanged();
    }

    public void ContinueLevel()
    {
        LevelStage = ELevelStage.Continued;
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
using System;

public class LevelStateChangedArgs : EventArgs { public int Level { get; set; } }
public class LevelFinishedEventArgs : LevelStateChangedArgs { public bool Success { get; set; } }
public delegate void LevelStateHandler(LevelStateChangedArgs _Args);
public delegate void LevelFinishedHandler(LevelFinishedEventArgs _Args);


public interface ILevelStagingModel
{
    int Level { get; set; }
    event LevelStateHandler LevelBeforeStarted;
    event LevelStateHandler LevelStarted;
    event LevelFinishedHandler LevelFinished;
    void BeforeStartLevel();
    void StartLevel();
    void FinishLevel(bool _Success);
 }

public abstract class LevelStagingModelBase : ILevelStagingModel
{
    public int Level { get; set; }
    
    public event LevelStateHandler LevelBeforeStarted;
    public event LevelStateHandler LevelStarted;
    public event LevelFinishedHandler LevelFinished;
    
    public virtual void BeforeStartLevel()
    {
        LevelBeforeStarted?.Invoke(new LevelStateChangedArgs {Level = Level});
    }

    public virtual void StartLevel()
    {
        LevelStarted?.Invoke(new LevelStateChangedArgs {Level = Level});
    }
        
    public virtual void FinishLevel(bool _Success)
    {
        LevelFinished?.Invoke(new LevelFinishedEventArgs{Level = Level, Success = _Success});
    }
}
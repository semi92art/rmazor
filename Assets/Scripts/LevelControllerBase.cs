public interface ILevelStagingModel
{
    int Level { get; set; }
    event LevelStateHandler OnLevelBeforeStarted;
    event LevelStateHandler OnLevelStarted;
    event LevelStateHandler OnLevelFinished;
    void BeforeStartLevel();
    void StartLevel();
    void FinishLevel();
 }

public abstract class LevelStagingModelBase : ILevelStagingModel
{
    public int Level { get; set; }
    
    public event LevelStateHandler OnLevelBeforeStarted;
    public event LevelStateHandler OnLevelStarted;
    public event LevelStateHandler OnLevelFinished;
    
    public virtual void BeforeStartLevel()
    {
        OnLevelBeforeStarted?.Invoke(new LevelStateChangedArgs {Level = Level});
    }

    public virtual void StartLevel()
    {
        OnLevelStarted?.Invoke(new LevelStateChangedArgs {Level = Level});
    }
        
    public virtual void FinishLevel()
    {
        OnLevelFinished?.Invoke(new LevelStateChangedArgs{Level = Level});
    }
}
public interface ILevelStagingModel
{
    int Level { get; set; }
    event LevelStateHandler LevelBeforeStarted;
    event LevelStateHandler LevelStarted;
    event LevelStateHandler LevelFinished;
    void BeforeStartLevel();
    void StartLevel();
    void FinishLevel();
 }

public abstract class LevelStagingModelBase : ILevelStagingModel
{
    public int Level { get; set; }
    
    public event LevelStateHandler LevelBeforeStarted;
    public event LevelStateHandler LevelStarted;
    public event LevelStateHandler LevelFinished;
    
    public virtual void BeforeStartLevel()
    {
        LevelBeforeStarted?.Invoke(new LevelStateChangedArgs {Level = Level});
    }

    public virtual void StartLevel()
    {
        LevelStarted?.Invoke(new LevelStateChangedArgs {Level = Level});
    }
        
    public virtual void FinishLevel()
    {
        LevelFinished?.Invoke(new LevelStateChangedArgs{Level = Level});
    }
}
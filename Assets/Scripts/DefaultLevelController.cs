using System.Collections.Generic;
using Managers;
using UnityEngine;

public interface ILevelController
{
    ICountdownController CountdownController { get; }
    int Level { get; set; }
    void StartLevel(float? _Duration, System.Func<bool> _StopPredicate);
    event LevelStateHandler OnLevelBeforeStarted;
    event LevelStateHandler OnLevelStarted;
    event LevelStateHandler OnLevelFinished;
    void BeforeStartLevel();
    void StartLevel();
    void FinishLevel();
 }

public class DefaultLevelController : ILevelController
{
    public ICountdownController CountdownController { get; }
    
    public int Level { get; set; }
    public Dictionary<MoneyType, long> Revenue { get; } = new Dictionary<MoneyType, long>();

    public event LevelStateHandler OnLevelBeforeStarted;
    public event LevelStateHandler OnLevelStarted;
    public event LevelStateHandler OnLevelFinished;

    public DefaultLevelController()
    {
        CountdownController = new CountdownController();
    }
        
    public virtual void StartLevel(float? _Duration, System.Func<bool> _StopPredicate)
    {
        if (_Duration.HasValue)
            CountdownController.SetDuration(_Duration.Value);
        CountdownController.StartCountdown(_StopPredicate);
        StartLevel();
    }

    public virtual void BeforeStartLevel()
    {
        OnLevelBeforeStarted?.Invoke(new LevelStateChangedArgs
        {
            Level = Level,
            Revenue = Revenue
        });
    }

    public virtual void StartLevel()
    {
        OnLevelStarted?.Invoke(new LevelStateChangedArgs
        {
            Level = Level,
            Revenue = Revenue
        });
    }
        
    public virtual void FinishLevel()
    {
        CountdownController.StopCountdown();
        OnLevelFinished?.Invoke(new LevelStateChangedArgs{
            Level = Level,
            Revenue = Revenue});
    }
}
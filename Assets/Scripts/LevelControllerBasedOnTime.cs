using System.Collections.Generic;
using Managers;

public interface ILevelController
{
    int Level { get; set; }
    Dictionary<MoneyType, long> Revenue { get; }
    int LifesLeft { get; set; }
    event LevelStateHandler OnLevelBeforeStarted;
    event LevelStateHandler OnLevelStarted;
    event LevelStateHandler OnLevelFinished;
    void BeforeStartLevel();
    void StartLevel();
    void FinishLevel();
}

public interface ILevelControllerBasedOnTime : ILevelController
{
    ICountdownController CountdownController { get; }
    void StartLevel(float _Duration, System.Func<bool> _StopPredicate);
}

public class LevelControllerBasedOnTime : DI.DiObject, ILevelControllerBasedOnTime
{
    public ICountdownController CountdownController { get; }
    
    public int Level { get; set; }
    public Dictionary<MoneyType, long> Revenue { get; } = new Dictionary<MoneyType, long>();
    public int LifesLeft { get; set; }
    
    public event LevelStateHandler OnLevelBeforeStarted;
    public event LevelStateHandler OnLevelStarted;
    public event LevelStateHandler OnLevelFinished;

    public LevelControllerBasedOnTime()
    {
        CountdownController = new CountdownController();
        CountdownController.OnTimeChange += TimeChanged;
    }
        
    public void StartLevel(float _Duration, System.Func<bool> _StopPredicate)
    {
        CountdownController.StartCountdown(_Duration, _StopPredicate);
        StartLevel();
    }

    public void BeforeStartLevel()
    {
        OnLevelBeforeStarted?.Invoke(new LevelStateChangedArgs
        {
            Level = Level,
            Revenue = Revenue
        });
    }

    public void StartLevel()
    {
        OnLevelStarted?.Invoke(new LevelStateChangedArgs
        {
            Level = Level,
            Revenue = Revenue
        });
    }
        
    public void FinishLevel()
    {
        CountdownController.StopCountdown();
        OnLevelFinished?.Invoke(new LevelStateChangedArgs{
            Level = Level,
            Revenue = Revenue});
    }

    private void TimeChanged(float _Time)
    {
        if (_Time <= 0)
            FinishLevel();
    }
}
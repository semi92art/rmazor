using System;
using System.Collections.Generic;
using Managers;
using Utils;

public interface ILevelController
{
    int Level { get; set; }
    Dictionary<MoneyType, long> Revenue { get; }
    int LifesLeft { get; set; }
    event LevelStateHandler OnLevelStarted;
    event LevelStateHandler OnLevelFinished;
    void StartLevel(Func<bool> _Predicate = null);
    void FinishLevel(Func<bool> _Predicate = null);
}

public class LevelControllerBasedOnTime : DI.ContainerObject, ILevelController
{
    protected ITimeController TimeController;
    
    public int Level { get; set; }
    public Dictionary<MoneyType, long> Revenue { get; } = new Dictionary<MoneyType, long>();
    public int LifesLeft { get; set; }
    public event LevelStateHandler OnLevelStarted;
    public event LevelStateHandler OnLevelFinished;

    public LevelControllerBasedOnTime()
    {
        if (TimeController == null)
            TimeController = new DefaultTimeController();
        TimeController.OnTimeFinished += () => FinishLevel();
    }
        
    public virtual void StartLevel(Func<bool> _Predicate = null)
    {
        Coroutines.Run(Coroutines.WaitWhile(() =>
        {
            OnLevelStarted?.Invoke(new LevelStateChangedArgs
            {
                Level = Level,
                LifesLeft = LifesLeft,
                Revenue = Revenue
            });
        }, () => _Predicate?.Invoke() ?? true));
    }
        
    public virtual void FinishLevel(Func<bool> _Predicate = null)
    {
        Coroutines.Run(Coroutines.WaitWhile(() =>
        {
            OnLevelFinished?.Invoke(new LevelStateChangedArgs{
                Level = Level,
                LifesLeft = LifesLeft,
                Revenue = Revenue});
        }, () => _Predicate?.Invoke() ?? true));
    }
}
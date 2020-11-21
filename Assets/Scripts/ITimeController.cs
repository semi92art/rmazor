using System;
using UnityEngine;

public delegate void TimeFinishedHandler();

public interface ITimeController
{
    event TimeFinishedHandler OnTimeFinished;
    float CurrentTime { get; }
    void StartTime(int _Level);
}

public class DefaultTimeController : DI.ContainerObject, ITimeController
{
    #region protected members

    protected bool DoUpdate;
    protected int Level;
    protected Func<bool> TimeStopPredicate;

    #endregion
    
    #region api
    
    public event TimeFinishedHandler OnTimeFinished;
    public float CurrentTime { get; protected set; }
    
    public virtual void StartTime(int _Level)
    {
        Level = _Level;
        DoUpdate = true;
        CurrentTime = 0;
    }

    [DI.Update(0)]
    private void OnUpdate()
    {
        if (!DoUpdate)
            return;
        CurrentTime = Time.time;
        if (TimeStopPredicate == null || !TimeStopPredicate())
            return;
        DoUpdate = false;
        OnTimeFinished?.Invoke();
    }
    
    #endregion
}



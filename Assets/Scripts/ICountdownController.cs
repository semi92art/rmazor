using UnityEngine;

public delegate void TimeHandler(float _Time);

public interface ICountdownController
{
    event TimeHandler OnTimeChange;
    event NoArgsHandler OnTimeEnded;
    void SetDuration(float _Duration);
    void StartCountdown(System.Func<bool> _StopPredicate);
    void StopCountdown();
}

public class CountdownController : DI.DiObject, ICountdownController
{
    #region protected members

    private bool m_DoUpdate;
    private System.Func<bool> m_StopPredicate;
    private float m_Duration;
    private float m_StartTime;

    #endregion
    
    #region api
    
    public event TimeHandler OnTimeChange;
    public event NoArgsHandler OnTimeEnded;

    public void SetDuration(float _Duration)
    {
        m_Duration = _Duration;
        OnTimeChange?.Invoke(m_Duration);
    }
    
    public void StartCountdown(System.Func<bool> _StopPredicate)
    {
        m_StartTime = Time.time;
        m_StopPredicate = _StopPredicate;
        m_DoUpdate = true;
    }

    public void StopCountdown()
    {
        m_DoUpdate = false;
    }

    [DI.Update]
    private void OnUpdate()
    {
        float timerLeft = m_Duration - (Time.time - m_StartTime);
        if (!m_DoUpdate)
            return;
        if (m_StopPredicate != null && m_StopPredicate() || timerLeft < 0)
        {
            StopCountdown();
            OnTimeEnded?.Invoke();
        }
        OnTimeChange?.Invoke(timerLeft);
    }
    
    #endregion
}



using UnityEngine;

public delegate void TimeHandler(float _Time);

public interface ICountdownController
{
    event TimeHandler OnTimeChange;
    void StartCountdown(float _Duration, System.Func<bool> _StopPredicate);
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
    
    public void StartCountdown(float _Duration, System.Func<bool> _StopPredicate)
    {
        m_Duration = _Duration;
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
        if (!m_DoUpdate)
            return;
        if (m_StopPredicate != null && m_StopPredicate() ||
            Time.time - m_StartTime > m_Duration)
            m_DoUpdate = false;
        OnTimeChange?.Invoke(m_Duration - (Time.time - m_StartTime));
    }
    
    #endregion
}



using System;

public class LifeEventArgs : EventArgs
{
    public long Lifes { get; }

    public LifeEventArgs(long _Lifes)
    {
        Lifes = _Lifes;
    }
}

public delegate void LifesChangedEventHandler(LifeEventArgs _Args);

public interface ILifesController
{
    event LifesChangedEventHandler OnLifesChanged;
    event NoArgsHandler OnLifesEnded;
    void Init(long _LifesOnStart);
    long Lifes { get; }
    void MinusOneLife();
    void PlusLifes(long _Count);
    void SetLifesWithoutNotification(long _Lifes);
}

public class DefaultLifesController : ILifesController
{
    private long m_Lifes;
    
    public event LifesChangedEventHandler OnLifesChanged;
    public event NoArgsHandler OnLifesEnded;

    public void Init(long _LifesOnStart)
    {
        Lifes = _LifesOnStart;
    }
    
    public long Lifes
    {
        get => m_Lifes;
        protected set
        {
            m_Lifes = Math.Max(0, value);
            OnLifesChanged?.Invoke(new LifeEventArgs(m_Lifes));
            if (m_Lifes <= 0)
                OnLifesEnded?.Invoke();
        }
    }
    public void MinusOneLife()
    {
        Lifes--;
    }

    public void PlusLifes(long _Count)
    {
        Lifes += _Count;
    }

    public void SetLifesWithoutNotification(long _Lifes)
    {
        m_Lifes = Math.Max(0, _Lifes);
    }
}

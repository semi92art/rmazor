using System;
using Managers;
using Utils;

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
    void Init(long _LifesOnStart);
    long Lifes { get; }
    void MinusOneLife();
    void PlusOneLife();
}

public class DefaultLifesController : ILifesController
{
    private long m_Lifes;
    
    public event LifesChangedEventHandler OnLifesChanged;

    public void Init(long _LifesOnStart)
    {
        Lifes = _LifesOnStart;
    }
    
    public long Lifes
    {
        get => m_Lifes;
        protected set
        {
            m_Lifes = value;
            OnLifesChanged?.Invoke(new LifeEventArgs(m_Lifes));
        }
    }
    public void MinusOneLife()
    {
        Lifes--;
    }

    public void PlusOneLife()
    {
        Lifes++;
    }
}

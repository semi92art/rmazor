using UnityEngine.Events;

public interface IInit
{
    bool Initialized { get; }
    event UnityAction Initialize;
    void Init();
}


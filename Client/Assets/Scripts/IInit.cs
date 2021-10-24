using UnityEngine.Events;

public interface IInit
{
    event UnityAction Initialized;
    void Init();
}


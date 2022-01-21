using UnityEngine.Events;

namespace Common
{
    public interface IInit
    {
        bool              Initialized { get; }
        event UnityAction Initialize;
        void              Init();
    }
}
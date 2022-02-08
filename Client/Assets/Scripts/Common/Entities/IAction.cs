using UnityEngine.Events;

namespace UI
{
    public interface IAction
    { 
        UnityAction Action { get; set; }
    }
}
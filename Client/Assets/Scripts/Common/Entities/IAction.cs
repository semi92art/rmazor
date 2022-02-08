using UnityEngine.Events;

namespace Common.Entities
{
    public interface IAction
    { 
        UnityAction Action { get; set; }
    }
}
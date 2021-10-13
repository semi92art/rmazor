using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IInputConfigurator : IInit
    {
        event UnityAction<int, object[]> Command; 
        bool Locked { get; set; }
    }
}
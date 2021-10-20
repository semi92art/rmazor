using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputConfigurator : IInit
    {
        event UnityAction<int, object[]> Command; 
        event UnityAction<int, object[]> ForcedCommand; 
        bool Locked { get; set; }
        void RaiseCommand(int _Key, object[] _Args);
    }
}
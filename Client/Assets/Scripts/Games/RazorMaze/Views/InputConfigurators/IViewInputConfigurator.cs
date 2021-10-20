using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputConfigurator : IInit
    {
        event UnityAction<int, object[]> Command; 
        event UnityAction<int, object[]> InternalCommand; 
        bool Locked { get; set; }
        bool RotationLocked { get; set; }
        void RaiseCommand(int _Key, object[] _Args, bool _Forced = false);
    }
}
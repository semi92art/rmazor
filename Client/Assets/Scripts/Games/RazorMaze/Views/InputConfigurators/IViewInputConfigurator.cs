using System.Collections.Generic;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputConfigurator : IInit
    {
        event UnityAction<int, object[]> Command; 
        event UnityAction<int, object[]> InternalCommand; 
        void LockCommand(int _Key);
        void UnlockCommand(int _Key);
        void LockCommands(IEnumerable<int> _Keys);
        void UnlockCommands(IEnumerable<int> _Keys);
        void LockAllCommands();
        void UnlockAllCommands();
        void RaiseCommand(int _Key, object[] _Args, bool _Forced = false);
    }
}
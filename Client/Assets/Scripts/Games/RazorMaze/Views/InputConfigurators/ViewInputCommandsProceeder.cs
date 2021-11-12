using System;
using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputCommandsProceeder
    {
        event UnityAction<EInputCommand, object[]> Command; 
        event UnityAction<EInputCommand, object[]> InternalCommand; 
        
        void LockCommand(EInputCommand _Key);
        void UnlockCommand(EInputCommand _Key);
        void LockCommands(IEnumerable<EInputCommand> _Keys);
        void UnlockCommands(IEnumerable<EInputCommand> _Keys);
        void LockAllCommands();
        void UnlockAllCommands();
        void RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false);
    }
    
    public class ViewInputCommandsProceeder : IViewInputCommandsProceeder
    {
        private readonly List<EInputCommand> m_LockedCommands = new List<EInputCommand>();
        
        public event UnityAction<EInputCommand, object[]> Command;
        public event UnityAction<EInputCommand, object[]> InternalCommand;
        
        public void LockCommand(EInputCommand _Key)
        {
            if (!m_LockedCommands.Contains(_Key))
                m_LockedCommands.Add(_Key);
        }

        public void UnlockCommand(EInputCommand _Key)
        {
            if (m_LockedCommands.Contains(_Key))
                m_LockedCommands.Remove(_Key);
        }

        public void LockCommands(IEnumerable<EInputCommand> _Keys)
        {
            foreach (var key in _Keys)
                LockCommand(key);
        }

        public void UnlockCommands(IEnumerable<EInputCommand> _Keys)
        {
            foreach (var key in _Keys)
                UnlockCommand(key);
        }

        public void LockAllCommands()
        {
            var allCommands = Enum.GetValues(typeof(EInputCommand)).Cast<EInputCommand>();
            LockCommands(allCommands);
        }

        public void UnlockAllCommands()
        {
            var allCommands = Enum.GetValues(typeof(EInputCommand)).Cast<EInputCommand>();
            UnlockCommands(allCommands);
        }
        
        public void RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false)
        {
            InternalCommand?.Invoke(_Key, _Args);
            if (!m_LockedCommands.Contains(_Key) || _Forced)
                Command?.Invoke(_Key, _Args);
        }
    }
}
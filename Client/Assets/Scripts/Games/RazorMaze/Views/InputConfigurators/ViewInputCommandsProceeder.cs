using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Games.RazorMaze.Models;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputCommandsProceeder
    {
        event UnityAction<EInputCommand, object[]> Command; 
        event UnityAction<EInputCommand, object[]> InternalCommand; 
        
        void                       LockCommand(EInputCommand _Key, string _Group = "common");
        void                       UnlockCommand(EInputCommand _Key, string _Group = "common");
        void                       LockCommands(IEnumerable<EInputCommand> _Keys, string _Group = "common");
        void                       UnlockCommands(IEnumerable<EInputCommand> _Keys, string _Group = "common");
        IEnumerable<EInputCommand> GetAllCommands();
        void                       UnlockAllCommands(string _Group = "common");
        void                       RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false);
    }
    
    public class ViewInputCommandsProceeder : IViewInputCommandsProceeder
    {
        private readonly Dictionary<string, List<EInputCommand>> m_LockedCommands = 
            new Dictionary<string, List<EInputCommand>>();
        
        public event UnityAction<EInputCommand, object[]> Command;
        public event UnityAction<EInputCommand, object[]> InternalCommand;
        
        public void LockCommand(EInputCommand _Key, string _Group = "common")
        {
            if (!m_LockedCommands.ContainsKey(_Group))
                m_LockedCommands.Add(_Group, new List<EInputCommand>());
            var commandsGroup = m_LockedCommands[_Group];
            if (!commandsGroup.Contains(_Key))
                commandsGroup.Add(_Key);
        }

        public void UnlockCommand(EInputCommand _Key, string _Group = "common")
        {
            if (_Group.EqualsIgnoreCase("all"))
            {
                foreach (var commands in m_LockedCommands.Values)
                    commands.Clear();
                return;
            }
            if (!m_LockedCommands.ContainsKey(_Group))
                return;
            var commandsGroup = m_LockedCommands[_Group];
            if (commandsGroup == null)
                return;
            if (commandsGroup.Contains(_Key))
                commandsGroup.Remove(_Key);
        }

        public void LockCommands(IEnumerable<EInputCommand> _Keys, string _Group = "common")
        {
            foreach (var key in _Keys)
                LockCommand(key, _Group);
        }

        public void UnlockCommands(IEnumerable<EInputCommand> _Keys, string _Group = "common")
        {
            foreach (var key in _Keys)
                UnlockCommand(key, _Group);
        }

        public IEnumerable<EInputCommand> GetAllCommands()
        {
            return Enum.GetValues(typeof(EInputCommand)).Cast<EInputCommand>();
        }

        public void UnlockAllCommands(string _Group = "common")
        {
            var allCommands = GetAllCommands();
            UnlockCommands(allCommands, _Group);
        }
        
        public void RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false)
        {
            InternalCommand?.Invoke(_Key, _Args);
            if (_Forced)
            {
                Command?.Invoke(_Key, _Args);
                return;
            }
            if (m_LockedCommands.Values.Any(_Group => _Group.Contains(_Key)))
                return;
            Command?.Invoke(_Key, _Args);
        }
    }
}
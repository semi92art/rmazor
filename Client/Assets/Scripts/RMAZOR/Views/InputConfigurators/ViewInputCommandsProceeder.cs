using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.InputConfigurators
{
    public interface IViewInputCommandsProceeder : IInit
    {
        event UnityAction<EInputCommand, object[]> Command; 
        event UnityAction<EInputCommand, object[]> InternalCommand; 
        
        void                       LockCommand(EInputCommand _Key, string _Group);
        void                       UnlockCommand(EInputCommand _Key, string _Group);
        void                       LockCommands(IEnumerable<EInputCommand> _Keys, string _Group);
        void                       UnlockCommands(IEnumerable<EInputCommand> _Keys, string _Group);
        IEnumerable<EInputCommand> GetAllCommands();
        void                       UnlockAllCommands(string _Group = "common");
        void                       RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false);
        bool                       IsCommandLocked(EInputCommand _Key);
    }
    
    public class ViewInputCommandsProceeder : IViewInputCommandsProceeder
    {
        #region nonpublic members

        public readonly Dictionary<string, List<EInputCommand>> m_LockedCommands = 
            new Dictionary<string, List<EInputCommand>>();

        private IEnumerable<EInputCommand> m_AllCommands;

        #endregion

        #region api

         public event UnityAction<EInputCommand, object[]> Command;
        public event UnityAction<EInputCommand, object[]> InternalCommand;
        
        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        public void Init()
        {
            m_AllCommands = Enum.GetValues(typeof(EInputCommand)).Cast<EInputCommand>();
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void LockCommand(EInputCommand _Key, string _Group)
        {
            if (!m_LockedCommands.ContainsKey(_Group))
                m_LockedCommands.Add(_Group, new List<EInputCommand>());
            var commandsGroup = m_LockedCommands[_Group];
            if (!commandsGroup.Contains(_Key))
                commandsGroup.Add(_Key);
        }

        public void UnlockCommand(EInputCommand _Key, string _Group)
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

        public void LockCommands(IEnumerable<EInputCommand> _Keys, string _Group)
        {
            foreach (var key in _Keys)
                LockCommand(key, _Group);
        }

        public void UnlockCommands(IEnumerable<EInputCommand> _Keys, string _Group)
        {
            foreach (var key in _Keys)
                UnlockCommand(key, _Group);
        }

        public IEnumerable<EInputCommand> GetAllCommands()
        {
            return m_AllCommands;
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

        public bool IsCommandLocked(EInputCommand _Key)
        {
            return m_LockedCommands.Values.Any(_Group => _Group.Contains(_Key));
        }

        #endregion
    }
}
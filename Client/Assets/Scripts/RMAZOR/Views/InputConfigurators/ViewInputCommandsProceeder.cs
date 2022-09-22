using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using RMAZOR.Models;
using UnityEngine.Events;
using Zenject;

namespace RMAZOR.Views.InputConfigurators
{
    public interface IViewInputCommandsProceeder : IInit
    {
        float                                      SecondsWithoutCommand { get; }
        event UnityAction<EInputCommand, object[]> Command; 
        event UnityAction<EInputCommand, object[]> InternalCommand; 
        
        void                       LockCommand(EInputCommand _Key, string _Group);
        void                       UnlockCommand(EInputCommand _Key, string _Group);
        void                       LockCommands(IEnumerable<EInputCommand> _Keys, string _Group);
        void                       UnlockCommands(IEnumerable<EInputCommand> _Keys, string _Group);
        IEnumerable<EInputCommand> GetAllCommands();
        void                       UnlockAllCommands(string _Group = "common");
        bool                       RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false);
        bool                       IsCommandLocked(EInputCommand _Key);
    }
    
    public class ViewInputCommandsProceeder : InitBase, IViewInputCommandsProceeder, IUpdateTick
    {
        #region nonpublic members

        public readonly Dictionary<string, List<EInputCommand>> LockedCommands = 
            new Dictionary<string, List<EInputCommand>>();

        private IEnumerable<EInputCommand> m_AllCommands;

        #endregion

        #region inject
        
        private ICommonTicker CommonTicker { get; }

        protected ViewInputCommandsProceeder() { }
        
        [Inject]
        protected ViewInputCommandsProceeder(ICommonTicker _CommonTicker)
        {
            CommonTicker = _CommonTicker;
        }
        
        #endregion

        #region api

        public float SecondsWithoutCommand { get; private set; }
        
        public event UnityAction<EInputCommand, object[]> Command;
        public event UnityAction<EInputCommand, object[]> InternalCommand;
        
        public override void Init()
        {
            CommonTicker?.Register(this);
            m_AllCommands = Enum.GetValues(typeof(EInputCommand)).Cast<EInputCommand>();
            base.Init();
        }
        
        public virtual void UpdateTick()
        {
            if (CommonTicker == null)
                return;
            SecondsWithoutCommand += CommonTicker.DeltaTime;
        }
        
        public void LockCommand(EInputCommand _Key, string _Group)
        {
            if (!LockedCommands.ContainsKey(_Group))
                LockedCommands.Add(_Group, new List<EInputCommand>());
            var commandsGroup = LockedCommands[_Group];
            if (!commandsGroup.Contains(_Key))
                commandsGroup.Add(_Key);
        }

        public void UnlockCommand(EInputCommand _Key, string _Group)
        {
            if (_Group.EqualsIgnoreCase("all"))
            {
                foreach (var commands in LockedCommands.Values)
                    commands.Clear();
                return;
            }
            if (!LockedCommands.ContainsKey(_Group))
                return;
            var commandsGroup = LockedCommands[_Group];
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
        
        public virtual bool RaiseCommand(EInputCommand _Key, object[] _Args, bool _Forced = false)
        {
            SecondsWithoutCommand = 0f;
            InternalCommand?.Invoke(_Key, _Args);
            if (_Forced)
            {
                Command?.Invoke(_Key, _Args);
                return true;
            }
            if (LockedCommands.Values.Any(_Group => _Group.Contains(_Key)))
                return false;
            Command?.Invoke(_Key, _Args);
            return true;
        }

        public bool IsCommandLocked(EInputCommand _Key)
        {
            return LockedCommands.Values.Any(_Group => _Group.Contains(_Key));
        }

        #endregion


    }
}
#if UNITY_EDITOR || DEVELOPMENT_BUILD

using System;
using System.Collections.Generic;
using Constants;
using Entities;
using UnityEngine;

namespace DebugConsole
{
    public class DebugConsoleController : GameObserver
    {
        #region event declarations

        public delegate void LogChangedHandler(string[] _Log);
        public event LogChangedHandler OnLogChanged;
        public delegate void VisibilityChangedHandler(bool _Visible);
        public event VisibilityChangedHandler VisibilityChanged;

        #endregion

        #region types
        
        public delegate void CommandHandler(string[] _Args);

        public class CommandRegistration
        {
            public string Command { get; }
            public CommandHandler Handler { get; }
            public string Description { get; }

            public CommandRegistration(string _Command, CommandHandler _Handler, string _Description)
            {
                Command = _Command;
                Handler = _Handler;
                Description = _Description;
            }
        }

        #endregion

        #region constants

        private const int ScrollbackSize = 100;

        #endregion

        #region public properties
        public string[] Log { get; private set; }
        public Queue<string> Scrollback { get; } = new Queue<string>(ScrollbackSize);
        public Dictionary<string, CommandRegistration> Commands { get;} = new Dictionary<string, CommandRegistration>();
        public List<string> CommandHistory { get; } = new List<string>();
        
        #endregion

        #region constructor

        public DebugConsoleController()
        {
            DebugConsoleCommands.Controller = this;
            DebugConsoleCommands.RegisterCommands();
        }

        #endregion

        #region public methods

        public void RaiseLogChangedEvent(string[] _Args)
        {
            OnLogChanged?.Invoke(_Args);
        }
        
        public void RegisterCommand(string _Command, CommandHandler _Handler, string _Description)
        {
            Commands.Add(_Command, new CommandRegistration(_Command, _Handler, _Description));
        }
        
        public void AppendLogLine(string _Line)
        {
            Debug.Log(_Line);

            if (Scrollback.Count >= ScrollbackSize)
                Scrollback.Dequeue();
            Scrollback.Enqueue(_Line);

            Log = Scrollback.ToArray();
            OnLogChanged?.Invoke(Log);
        }
        
        public void RunCommandString(string _CommandString)
        {
            AppendLogLine("$ " + _CommandString);

            string[] commandSplit = ParseArguments(_CommandString);
            string[] args = new string[0];
            if (commandSplit.Length <= 0)
                return;

            if (commandSplit.Length >= 2)
            {
                int numArgs = commandSplit.Length - 1;
                args = new string[numArgs];
                Array.Copy(commandSplit, 1, args, 0, numArgs);
            }
            RunCommand(commandSplit[0].ToLower(), args);
            CommandHistory.Add(_CommandString);
        }

        #endregion

        #region nonpublic methods
        
        

        private void RunCommand(string _Command, string[] _Args)
        {
            if (!Commands.TryGetValue(_Command, out var reg))
                AppendLogLine($"Unknown command '{_Command}', type 'help' for list.");
            else
            {
                if (reg.Handler == null)
                    AppendLogLine($"Unable to process command '{_Command}', handler was null.");
                else
                    reg.Handler(_Args);
            }
        }

        private static string[] ParseArguments(string _CommandString)
        {
            LinkedList<char> parmChars = new LinkedList<char>(_CommandString.ToCharArray());
            bool inQuote = false;
            var node = parmChars.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value == '"')
                {
                    inQuote = !inQuote;
                    parmChars.Remove(node);
                }
                if (!inQuote && node.Value == ' ')
                {
                    node.Value = ' ';
                }
                node = next;
            }
            char[] parmCharsArr = new char[parmChars.Count];
            parmChars.CopyTo(parmCharsArr, 0);
            return (new string(parmCharsArr)).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }
        
        protected override void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            if (_NotifyMessage != CommonNotifyMessages.RegisterCommand || _Args.Length < 3)
                return;

            var command = _Args[0] as string;
            var handler = _Args[1] as CommandHandler;
            string description = string.Empty;
            if (_Args.Length > 2)
                description = _Args[2] as string;
            
            if (!string.IsNullOrEmpty(command) && handler != null)
                RegisterCommand(command, handler, description);
        }

        #endregion
    }
}

#endif

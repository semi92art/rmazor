using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Lean.Localization;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

//using Boo.Lang;

namespace DebugConsole
{
    public class DebugConsoleController
    {
        #region Event declarations

        public delegate void LogChangedHandler(string[] _Log);
        public event LogChangedHandler LogChanged;
        public delegate void VisibilityChangedHandler(bool _Visible);
        public event VisibilityChangedHandler VisibilityChanged;

        #endregion

        #region types

        private delegate void CommandHandler(string[] _Args);

        private class CommandRegistration
        {
            public string Command { get; }
            public CommandHandler Handler { get; }
            public string Help { get; }

            public CommandRegistration(string _Command, CommandHandler _Handler, string _Help)
            {
                Command = _Command;
                Handler = _Handler;
                Help = _Help;
            }
        }

        #endregion

        #region constants

        const int SCROLLBACK_SIZE = 100;

        #endregion

        #region public properties
        public string[] Log { get; private set; }

        #endregion

        #region readonly
        public readonly List<string> CommandHistory = new List<string>();
        //public readonly List CommandHistory = new List();
        private readonly Queue<string> m_Scrollback = new Queue<string>(SCROLLBACK_SIZE);
        private readonly Dictionary<string, CommandRegistration> m_Commands = new Dictionary<string, CommandRegistration>();

        #endregion

        #region consturctors

        public DebugConsoleController()
        {
            //When adding commands, you must add a call below to registerCommand() with its name, implementation method, and help text.
            RegisterCommand("help", Help, "Print command list.");
            RegisterCommand("restart", Restart, "Restart game.");
            RegisterCommand("load", Load, "Reload specified level.");
            RegisterCommand("reload", Reload, "Reload current level.");
            RegisterCommand("clc", ClearConsole, "Clear console.");
            RegisterCommand("sw_sound", SwitchSound,"Switch sound.");
            RegisterCommand("set_lang",SetLanguage,"set language");
        }

        #endregion

        #region private methods

        private void RegisterCommand(string _Command, CommandHandler _Handler, string _Help)
        {
            m_Commands.Add(_Command, new CommandRegistration(_Command, _Handler, _Help));
        }

        private void AppendLogLine(string _Line)
        {
            Debug.Log(_Line);

            if (m_Scrollback.Count >= SCROLLBACK_SIZE)
                m_Scrollback.Dequeue();
            m_Scrollback.Enqueue(_Line);

            Log = m_Scrollback.ToArray();
            if (LogChanged != null)
                LogChanged(Log);
        }

        private void RunCommand(string _Command, string[] _Args)
        {
            CommandRegistration reg = null;
            if (!m_Commands.TryGetValue(_Command, out reg))
                AppendLogLine(string.Format("Unknown command '{0}', type 'help' for list.", _Command));
            else
            {
                if (reg.Handler == null)
                    AppendLogLine(string.Format("Unable to process command '{0}', handler was null.", _Command));
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

        #endregion

        #region public methods

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

        #region Command handlers
        //Implement new commands in this region of the file.

        private void Reload(string[] _Args)
        {
            if (_Args.Length == 0)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            else if (_Args.Any(_Arg => _Arg == "-h"))
                AppendLogLine("Reload current level.");
        }

        private void Help(string[] _Args)
        {
            if (_Args.Length == 0)
            {
                Debug.Log("Print: command list");
                AppendLogLine("Current command list:");
                foreach (KeyValuePair<string, CommandRegistration> kvp in m_Commands)
                    AppendLogLine($"{kvp.Key}: {kvp.Value.Help}");
            }
            else if (_Args.Any(_Arg => _Arg == "woodpecker"))
                AppendLogLine("Work is filled up, woodpeckers!");
        }

        private void Restart(string[] _Args)
        {
            if (_Args.Length == 0)
                SceneManager.LoadScene(0);
            else if (_Args.Any(_Arg => _Arg == "-h"))
                AppendLogLine("Reload game.");
        }

        private void Load(string[] _Args)
        {
            if (_Args.Length == 0)
                AppendLogLine("Specify level");
            else
            {
                foreach (string arg in _Args)
                {
                    if (arg == "-h")
                    {
                        AppendLogLine("Load level:");
                        AppendLogLine("preload: restart game.");
                        AppendLogLine("menu: load menu.");
                        AppendLogLine("level: load field.");
                        break;
                    }

                    switch (arg)
                    {
                        case "preload":
                            SceneManager.LoadScene(SceneNames.Preload);
                            break;
                        case "menu":
                            SceneManager.LoadScene(SceneNames.Main);
                            break;
                        case "level":
                            SceneManager.LoadScene(SceneNames.Level);
                            break;
                        default:
                            AppendLogLine("No such level.");
                            break;
                    }
                }
            }
        }

        void SwitchSound(string[] _Args)
        {
            if (_Args.Length == 0)
                if (Utils.SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn))
                {
                    SoundManager.Instance.SwitchSound(false);
                    SoundManager.Instance.SwitchSoundInActualClips(false);
                }
                else
                {
                    SoundManager.Instance.SwitchSound(true);
                    SoundManager.Instance.SwitchSoundInActualClips(true);
                }
            else
            {
                foreach (string arg in _Args)
                {
                    if (arg == "-h")
                    {
                        AppendLogLine("on: switches sound on");
                        AppendLogLine("off: switches sound off");
                        break;
                    }

                    switch (arg)
                    {
                        case "on":
                            SoundManager.Instance.SwitchSound(true);
                            SoundManager.Instance.SwitchSoundInActualClips(true);
                            break;
                        case "off":
                            SoundManager.Instance.SwitchSound(false);
                            SoundManager.Instance.SwitchSoundInActualClips(false);
                            break;
                    }
                }
            }
        }

        void ClearConsole(string[] _Args)
        {
            Array.Clear(Log, 0, Log.Length);
            m_Scrollback.Clear();
            if (LogChanged != null)
                LogChanged(Log);
        }

        void SetLanguage(string[] _Args)
        {
            if (_Args.Length == 0)
                AppendLogLine("Specify language");
            else
            {
                foreach (string arg in _Args)
                {
                    if (arg == "-h")
                    {
                        AppendLogLine("Current language list:");
                        foreach (var lang in Enum.GetValues(typeof(Language)))
                        {
                            AppendLogLine(lang.ToString());
                        }
                        break;
                    }

                    LeanLocalization.CurrentLanguage = arg;
                }
            }  
        }
        #endregion
    }
}

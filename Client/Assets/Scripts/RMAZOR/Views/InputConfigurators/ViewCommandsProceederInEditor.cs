using System.Collections.Generic;
using Common;
using Lean.Common;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using Newtonsoft.Json;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Controllers;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.InputConfigurators
{
    public class ViewCommandsProceederInEditor : ViewInputCommandsProceeder
    {
        #region nonpublic members

        private bool m_DoProceed = true;

        #endregion
        
        #region inject

        private IModelGame Model  { get; }
        private IUITicker  Ticker { get; }

        private ViewCommandsProceederInEditor(IModelGame _Model, IUITicker _UITicker)
        {
            Model  = _Model;
            Ticker = _UITicker;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (!MazorCommonData.Release)
                m_DoProceed = true;
            Ticker.Register(this);
            base.Init();
        }

        public override void UpdateTick()
        {
            base.UpdateTick();
            if (!m_DoProceed)
                return;
            const bool forced = true;
            EInputCommand? commandKey = null;
            var args = new Dictionary<string, object>();
            ProceedMovement(ref commandKey);
            ProceedRotation(ref commandKey);
            ProceedLevelStatement(ref commandKey, args);
            ProceedCommandsRecording(ref commandKey, args);
            if (commandKey.HasValue)
                RaiseCommand(commandKey.Value, args, forced);
        }

        public override bool RaiseCommand(
            EInputCommand              _Key,
            Dictionary<string, object> _Args,
            bool                       _Forced = false)
        {
            m_DoProceed = _Key switch
            {
                EInputCommand.EnableDebug  => true,
                EInputCommand.DisableDebug => false,
                _                          => m_DoProceed
            };
            return base.RaiseCommand(_Key, _Args, _Forced);
        }

        #endregion

        #region nonpublic methods

        private static void ProceedMovement(ref EInputCommand? _CommandKey)
        {
            ProceedInput(EInputCommand.MoveLeft,  ref _CommandKey, KeyCode.A, KeyCode.LeftArrow);
            ProceedInput(EInputCommand.MoveRight, ref _CommandKey, KeyCode.D, KeyCode.RightArrow);
            ProceedInput(EInputCommand.MoveUp,    ref _CommandKey, KeyCode.W, KeyCode.UpArrow);
            ProceedInput(EInputCommand.MoveDown,  ref _CommandKey, KeyCode.S, KeyCode.DownArrow);
        }

        private static void ProceedRotation(ref EInputCommand? _CommandKey)
        {
            ProceedInput(EInputCommand.RotateClockwise,        ref _CommandKey, KeyCode.E);
            ProceedInput(EInputCommand.RotateCounterClockwise, ref _CommandKey, KeyCode.Q);
        }

        private void ProceedLevelStatement(
            ref EInputCommand?         _CommandKey,
            Dictionary<string, object> _Arguments)
        {
            ProceedInput(KeyCode.Alpha1, EInputCommand.LoadLevel, ref _CommandKey,  () =>
            {
                long levelIndex = Model.LevelStaging.LevelIndex + (LeanInput.GetPressed(KeyCode.LeftShift) ? 1 : 0);
                _Arguments.SetSafe(ComInComArg.KeyLevelIndex, levelIndex);
            });
            ProceedInput(EInputCommand.ReadyToStartLevel, ref _CommandKey, KeyCode.Alpha2);
            ProceedInput(EInputCommand.FinishLevel,       ref _CommandKey, KeyCode.Alpha3);
            ProceedInput(EInputCommand.PauseLevel,        ref _CommandKey, KeyCode.Alpha4);
        }

        private void ProceedCommandsRecording(
            ref EInputCommand?         _CommandKey,
            Dictionary<string, object> _Arguments)
        {
            ProceedInput(EInputCommand.StartRecordCommands,  ref _CommandKey, KeyCode.Alpha5);
            ProceedInput(EInputCommand.StopRecordCommands,   ref _CommandKey, KeyCode.Alpha6);
            ProceedInput(EInputCommand.GetRecordedCommands,  ref _CommandKey, KeyCode.Alpha7);
            ProceedInput(
                EInputCommand.PlayRecordedCommands,
                ref _CommandKey,
                () =>
                {
                    string passCommandsSerialized = GUIUtility.systemCopyBuffer;
                    var passCommandsRecord = JsonConvert.DeserializeObject<InputCommandsRecord>(passCommandsSerialized);
                    Model.Data.Info.AdditionalInfo.PassCommandsRecord = passCommandsRecord;
                    _Arguments.SetSafe(
                        ComInComArg.KeyPassCommandsRecord,
                        Model.Data.Info.AdditionalInfo.PassCommandsRecord);
                },
                KeyCode.Alpha8);
        }

        private static void ProceedInput(
            EInputCommand         _InputCommand,
            ref    EInputCommand? _ExistingCommand,
            params KeyCode[]      _KeyCodes)
        {
            ProceedInput(_InputCommand, ref _ExistingCommand, null, _KeyCodes);
        }

        private static void ProceedInput(
            EInputCommand      _InputCommand,
            ref EInputCommand? _ExistingCommand,
            UnityAction        _Action,
            params KeyCode[]   _KeyCodes)
        {
            foreach (var keyCode in _KeyCodes)
                ProceedInput(keyCode, _InputCommand, ref _ExistingCommand, _Action);
        }
        
        private static void ProceedInput(
            KeyCode                        _KeyCode,
            EInputCommand                  _InputCommand,
            ref EInputCommand?             _ExistingCommand,
            UnityAction                    _Action)
        {
            if (_ExistingCommand.HasValue)
                return;
            bool isKeyDown = IsKeyDown(_KeyCode);
            if (isKeyDown)
                _Action?.Invoke();
            _ExistingCommand = isKeyDown? _InputCommand : (EInputCommand?)null;
        }
        
        private static bool IsKeyDown(KeyCode _KeyCode)
        {
            return LeanInput.GetDown(_KeyCode);
        }

        #endregion
    }
}
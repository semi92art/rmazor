using System.Collections.Generic;
using Common;
using Lean.Common;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using Newtonsoft.Json;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.InputConfigurators
{
    public class ViewCommandsProceederInEditor : ViewCommandsProceederWithKeyboardBase
    {
        #region nonpublic members

        private bool m_DoProceed = true;

        #endregion
        
        #region inject

        private IModelGame Model  { get; }

        private ViewCommandsProceederInEditor(IModelGame _Model, ICommonTicker _CommonTicker) 
            : base(_CommonTicker)
        {
            Model  = _Model;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (!CommonDataMazor.Release)
                m_DoProceed = true;
            base.Init();
        }

        public override void UpdateTick()
        {
            UpdateTimeFromLastCommand();
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

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Controllers
{
    public interface IViewInputCommandsRecorder : IInit { }
    
    public class ViewInputCommandsRecorder : InitBase, IViewInputCommandsRecorder
    {
        #region nonpublic members

        private InputCommandsRecord m_InputCommandsRecord;
        private bool                m_DoRecord;
        private float               m_RecordStartTime;

        #endregion
        
        #region inject

        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IViewGameTicker             ViewGameTicker    { get; }

        public ViewInputCommandsRecorder(
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker             _ViewGameTicker)
        {
            CommandsProceeder = _CommandsProceeder;
            ViewGameTicker    = _ViewGameTicker;
        }

        #endregion

        #region api

        public override void Init()
        {
            CommandsProceeder.Command += OnCommand;
            base.Init();
        }
        
        #endregion

        #region nonpublic methods

        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            ProceedRecorderCommand(_Command);
            RecordCommand(_Command);
        }

        private void ProceedRecorderCommand(EInputCommand _Command)
        {
            switch (_Command)
            {
                case EInputCommand.StartRecordCommands:
                    Start();
                    break;
                case EInputCommand.StopRecordCommands:
                    Stop();
                    break;
                case EInputCommand.GetRecordedCommands:
                    var records = GetRecords();
                    string recordsSerialized = JsonConvert.SerializeObject(records);
                    CommonUtils.CopyToClipboard(recordsSerialized);
                    break;
            }
        }

        private void RecordCommand(EInputCommand _Command)
        {
            if (!m_DoRecord)
                return;
            if (!GetValidCommandsToRecord().Contains(_Command))
                return;
            float seconds = ViewGameTicker.Time - m_RecordStartTime;
            var record = new InputCommandRecord
            {
                Command = _Command,
                Span = TimeSpan.FromSeconds(seconds)
            };
            m_InputCommandsRecord.Records.Add(record);
        }
            
        private static IEnumerable<EInputCommand> GetValidCommandsToRecord()
        {
            return RmazorUtils.MoveAndRotateCommands;
        }
        
        private void Start()
        {
            m_InputCommandsRecord = new InputCommandsRecord();
            m_RecordStartTime     = ViewGameTicker.Time;
            m_DoRecord            = true;
        }

        private void Stop()
        {
            m_DoRecord = false;
        }

        private InputCommandsRecord GetRecords()
        {
            var firstSpan = m_InputCommandsRecord.Records[0].Span;
            foreach (var record in m_InputCommandsRecord.Records.ToList())
                record.Span -= firstSpan;
            return m_InputCommandsRecord;
        }

        #endregion
    }
}
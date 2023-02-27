using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Models.MazeInfos
{
    [Serializable]
    public class InputCommandRecord
    {
        [JsonIgnore, SerializeField] private EInputCommand command;
        [JsonIgnore, SerializeField] private double        spanSecs;

        [JsonProperty(PropertyName = "P")]
        public EInputCommand Command
        {
            get => command;
            set => command = value;
        }

        [JsonProperty(PropertyName = "S")]
        public TimeSpan Span
        {
            get => TimeSpan.FromSeconds(spanSecs);
            set => spanSecs = value.TotalSeconds;
        }
    }

    [Serializable]
    public class InputCommandsRecord
    {
        [JsonIgnore, SerializeField] private List<InputCommandRecord> records = new List<InputCommandRecord>();

        [JsonProperty(PropertyName = "R")]
        public List<InputCommandRecord> Records
        {
            get => records;
            set => records = value;
        }
    }
}
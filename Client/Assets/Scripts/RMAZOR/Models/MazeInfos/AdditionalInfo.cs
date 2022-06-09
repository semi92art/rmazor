using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Models.MazeInfos
{
    [Serializable]
    public class AdditionalInfo
    {
        [JsonIgnore] [SerializeField] private string arguments;
        [JsonIgnore] [SerializeField] private string comment;
    
        [JsonProperty(PropertyName = "C1")]
        public string Arguments
        {
            get => arguments;
            set => arguments = value;
        }

        [JsonProperty(PropertyName = "C2")]
        public string Comment
        {
            get => comment;
            set => comment = value;
        }
    }
}
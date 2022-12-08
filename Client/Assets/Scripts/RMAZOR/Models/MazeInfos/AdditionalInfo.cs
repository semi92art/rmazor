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
        [JsonIgnore] [SerializeField] private float  time3Stars, time2Stars, time1Star;
        
    
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

        [JsonProperty(PropertyName = "T1")]
        public float Time3Stars
        {
            get => time3Stars;
            set => time3Stars = value;
        }
        
        [JsonProperty(PropertyName = "T2")]
        public float Time2Stars
        {
            get => time2Stars;
            set => time2Stars = value;
        }
        
        [JsonProperty(PropertyName = "T3")]
        public float Time1Star
        {
            get => time1Star;
            set => time1Star = value;
        }
    }
}
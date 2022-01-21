using System;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Models.MazeInfos
{
    [Serializable]
    public class AdditionalInfo
    {
        [JsonIgnore] [SerializeField] private string comment1;
        [JsonIgnore] [SerializeField] private string comment2;
    
        [JsonProperty(PropertyName = "C1")]
        public string Comment1
        {
            get => comment1;
            set => comment1 = value;
        }

        [JsonProperty(PropertyName = "C2")]
        public string Comment2
        {
            get => comment2;
            set => comment2 = value;
        }
    }
}
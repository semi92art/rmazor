using System;
using Common.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Models.MazeInfos
{
    [Serializable]
    public class PathItem
    {
        [JsonIgnore] [SerializeField] private V2Int position;
        [JsonIgnore] [SerializeField] private bool  blank;

        [JsonProperty(PropertyName = "P")]
        public V2Int Position
        {
            get => position;
            set => position = value;
        }

        [JsonProperty(PropertyName = "E")]
        public bool Blank
        {
            get => blank;
            set => blank = value;
        }
    }
}
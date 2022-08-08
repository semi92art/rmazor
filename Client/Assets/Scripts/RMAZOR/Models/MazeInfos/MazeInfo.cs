using System;
using System.Collections.Generic;
using Common.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Models.MazeInfos
{
    [Serializable]
    public class MazeInfo
    {
        [JsonIgnore] [SerializeField] private V2Int          size;
        [JsonIgnore] [SerializeField] private List<PathItem> pathItems = new List<PathItem>();
        [JsonIgnore] [SerializeField] private List<MazeItem> mazeItems = new List<MazeItem>();
        [JsonIgnore] [SerializeField] private AdditionalInfo additionalInfo = new AdditionalInfo();
        
        public V2Int Size
        {
            get => size;
            set => size = value;
        }

        public List<MazeItem> MazeItems
        {
            get => mazeItems;
            set => mazeItems = value;
        }

        [JsonProperty(PropertyName = "P")]
        public List<PathItem> PathItems
        {
            get => pathItems;
            set => pathItems = value;
        }

        [JsonProperty(PropertyName = "I")]
        public AdditionalInfo AdditionalInfo
        {
            get => additionalInfo;
            set => additionalInfo = value;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Size.GetHashCode();
                foreach (var item in MazeItems)
                    hash = hash * 23 + item.GetHashCode();
                foreach (var item in PathItems)
                    hash = hash * 23 + item.GetHashCode();
                return hash;
            }
        }
    }
}
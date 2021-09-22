using System;
using System.Collections.Generic;
using Entities;
using Newtonsoft.Json;

namespace Games.RazorMaze.Models
{
    [Serializable]
    public class MazeInfo
    {
        public V2Int Size { get; set; }
        public List<V2Int> Path { get; set; } = new List<V2Int>();
        public List<MazeItem> MazeItems { get; set; } = new List<MazeItem>();
        [JsonProperty(PropertyName = "C")] public string Comment { get; set; }
    }
}
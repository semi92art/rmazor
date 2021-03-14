using Entities;
using Newtonsoft.Json;

namespace Games.RazorMaze.Models
{
    public class Node
    {
        [JsonProperty(PropertyName = "P")] public V2Int Position { get; set; }
    }
}
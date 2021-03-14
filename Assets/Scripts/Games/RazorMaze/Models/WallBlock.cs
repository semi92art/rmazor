using Entities;
using Newtonsoft.Json;

namespace Games.RazorMaze.Models
{
    public enum EWalBlockType
    {
        Simple,
        Obstacle,
        Trap,
        MovableTrap
    }
    
    public class WallBlock
    {
        [JsonProperty(PropertyName = "P")] public V2Int Position { get; set; }
        [JsonProperty(PropertyName = "T")] public EWalBlockType Type { get; set; } = EWalBlockType.Simple;
        [JsonProperty(PropertyName = "E")] public V2Int EndPosition { get; set; }
        [JsonProperty(PropertyName = "D")] public V2Int DropDirection { get; set; }
    }
}
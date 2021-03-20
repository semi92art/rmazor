using System.Collections.Generic;
using Entities;
using Newtonsoft.Json;

namespace Games.RazorMaze.Models
{
    public enum EObstacleType
    {
        Obstacle,
        ObstacleMoving,
        Trap,
        TrapMoving
    }
    
    public class Obstacle
    {
        [JsonProperty(PropertyName = "P")] public V2Int Position { get; set; }
        [JsonProperty(PropertyName = "T")] public EObstacleType Type { get; set; } = EObstacleType.Obstacle;
        [JsonProperty(PropertyName = "W")] public List<V2Int> Path { get; set; } = new List<V2Int>();
    }
}
using System.Collections.Generic;
using Entities;
using Newtonsoft.Json;

namespace Games.RazorMaze.Models
{
    public enum EMazeItemType
    {
        // blocks
        Block,
        GravityBlock,
        ShredingerBlock,
        Portal,
        // traps
        TrapReact,
        TrapIncreasing,
        TrapMoving,
        GravityTrap,
        // turrets
        Turret,
        TurretRotating,
        Springboard
    }

    public enum EMazeItemMoveByPathDirection
    {
        Forward, 
        Backward
    }
    
    public class MazeItem
    {
        [JsonProperty(PropertyName = "T")] public EMazeItemType Type { get; set; } = EMazeItemType.Block;
        [JsonProperty(PropertyName = "P")] public V2Int Position { get; set; }
        [JsonProperty(PropertyName = "W")] public List<V2Int> Path { get; set; } = new List<V2Int>();
        [JsonProperty(PropertyName = "D")] public V2Int Direction { get; set; }
        [JsonProperty(PropertyName = "2")] public V2Int Pair { get; set; }
    }
}
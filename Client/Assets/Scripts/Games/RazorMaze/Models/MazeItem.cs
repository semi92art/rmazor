using System.Collections.Generic;
using Entities;
using Newtonsoft.Json;

namespace Games.RazorMaze.Models
{
    public enum EMazeItemType
    {
        Block,
        GravityBlock,
        ShredingerBlock,
        Portal,
        TrapReact,
        TrapIncreasing,
        TrapMoving,
        GravityTrap,
        Turret,
        GravityBlockFree,
        Springboard,
        MovingBlockFree
    }

    public enum EMoveByPathDirection
    {
        Forward, 
        Backward
    }
    
    public class MazeItem
    {
        [JsonProperty(PropertyName = "T")] public EMazeItemType Type { get; set; } = EMazeItemType.Block;
        [JsonProperty(PropertyName = "P")] public V2Int Position { get; set; }
        [JsonProperty(PropertyName = "W")] public List<V2Int> Path { get; set; } = new List<V2Int>();
        [JsonProperty(PropertyName = "D2")] public List<V2Int> Directions { get; set; } = new List<V2Int>();
        [JsonProperty(PropertyName = "D")] public V2Int Direction { get; set; }
        [JsonProperty(PropertyName = "2")] public V2Int Pair { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Position.GetHashCode();
                hash = hash * 23 + Directions.GetHashCode();
                hash = hash * 23 + Pair.GetHashCode();
                foreach (var item in Path)
                    hash = hash * 23 + item.GetHashCode();
                return hash;
            }
        }
    }
}
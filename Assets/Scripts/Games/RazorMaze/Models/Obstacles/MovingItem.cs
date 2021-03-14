using System.Collections.Generic;
using Entities;

namespace Games.RazorMaze.Models.Obstacles
{
    public enum MovingItemType { Block, Enemy }
    
    public class MovingItem
    {
        public MovingItemType Type => MovingItemType.Block;
        public V2Int Position { get; set; }
        public List<V2Int> Joints { get; }
        public event Vector2IntVector2IntHandler OnPositionChanged;

        public MovingItem(List<V2Int> _Joints)
        {
            Joints = _Joints;
            Position = _Joints[0];
        }
    }
}
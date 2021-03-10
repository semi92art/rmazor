using UnityEngine;

namespace Games.RazorMaze.WallBlocks
{
    public class WallBlockTrap : WallBlockBase
    {
        public Vector2 DropDirection { get; }
        
        public WallBlockTrap(Vector2Int _Position, Vector2Int _DropDirection) 
            : base(_Position, EWalBlockType.Trap)
        {
            DropDirection = _DropDirection;
        }
    }
}
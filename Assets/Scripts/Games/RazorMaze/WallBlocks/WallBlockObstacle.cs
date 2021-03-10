using UnityEngine;

namespace Games.RazorMaze.WallBlocks
{
    public class WallBlockObstacle : WallBlockBase
    {
        public Vector2Int EndPosition { get; }
        
        public WallBlockObstacle(Vector2Int _Position, Vector2Int _EndPosition) 
            : base(_Position, EWalBlockType.Obstacle)
        {
            EndPosition = _EndPosition;
        }
    }
}
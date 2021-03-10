using UnityEngine;

namespace Games.RazorMaze.WallBlocks
{
    public class WallBlockSimple : WallBlockBase
    {
        public WallBlockSimple(Vector2Int _Position) : base(_Position, EWalBlockType.Simple) { }
    }
}
using UnityEngine;

namespace Games.RazorMaze.WallBlocks
{
    public enum EWalBlockType
    {
        Simple,
        Obstacle,
        Trap,
        MovableTrap
    }

    public interface IWallBlock
    {
        Vector2Int Position { get; }
        EWalBlockType Type { get; }
    }
    
    public abstract class WallBlockBase : IWallBlock
    {
        public Vector2Int Position { get; }
        public EWalBlockType Type { get; }

        protected WallBlockBase(Vector2Int _Position, EWalBlockType _Type)
        {
            Position = _Position;
            Type = _Type;
        }
    }
}
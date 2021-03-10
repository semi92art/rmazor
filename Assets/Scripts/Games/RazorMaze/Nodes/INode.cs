using UnityEngine;

namespace Games.RazorMaze.Nodes
{
    public interface INode
    {
        Vector2Int Position { get; }
    }

    public class Node : INode
    {
        public Vector2Int Position { get; }

        public Node(Vector2Int _Position)
        {
            Position = _Position;
        }
    }
}
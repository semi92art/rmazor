using UnityEngine;

namespace Games.RazorMaze.Nodes
{
    public class NodeStart : Node
    {
        public bool Start = true;
        
        public NodeStart(Vector2Int _Position) : base(_Position) { }
    }
}
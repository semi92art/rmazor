using Exceptions;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Prot
{
    [ExecuteInEditMode]
    public class MazeProtItem : MonoBehaviour
    {
        public Rectangle rectangle;
        public PrototypingItemType type;
        private PrototypingItemType m_TypeCheck;
        private bool m_Initialized;

        private void Update()
        {
            if (!m_Initialized)
                return;
            if (type != m_TypeCheck)
                rectangle.Color = ColorByType(type);
            m_TypeCheck = type;
        }

        public void Init(PrototypingItemProps _Props)
        {
            type = _Props.Type;
            rectangle = gameObject.AddComponent<Rectangle>();
            rectangle.Width = 0.97f * _Props.Scale;
            rectangle.Height = 0.97f * _Props.Scale;
            rectangle.Type = Rectangle.RectangleType.RoundedSolid;
            rectangle.CornerRadius = 0.1f;
            rectangle.Color = ColorByType(type);
            m_Initialized = true;
        }

        private static Color ColorByType(PrototypingItemType _Type)
        {
            switch (_Type)
            {
                case PrototypingItemType.Node:                  return Color.white;
                case PrototypingItemType.NodeStart:             return Color.yellow;
                case PrototypingItemType.WallBlockSimple:       return Color.gray;
                case PrototypingItemType.WallBlockObstacle:     return Color.blue;
                case PrototypingItemType.WallBlockTrap:         return Color.red;
                case PrototypingItemType.WallBlockMovingTrap:   return Color.magenta;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }
    }
    
    public enum PrototypingItemType
    {
        Node,
        NodeStart,
        WallBlockSimple,
        WallBlockObstacle,
        WallBlockTrap,
        WallBlockMovingTrap
    }
    
    public class PrototypingItemProps
    {
        public int Index { get; set; }
        public PrototypingItemType Type { get; set; }
        public Vector2Int? Direction { get; set; }
        
        public float Scale { get; set; } 
    }
}
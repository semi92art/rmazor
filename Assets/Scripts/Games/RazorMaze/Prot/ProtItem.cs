using Exceptions;
using Extensions;
using Shapes;
using TMPro;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Prot
{
    [ExecuteInEditMode]
    public class ProtItem : MonoBehaviour
    {
        

        public TextMeshPro index;
        public Rectangle rectangle;
        public PrototypingItemType type;

        private PrototypingItemType m_TypeCheck;
        private bool m_Initialized;

        private void Update()
        {
            if (!m_Initialized)
                return;
            if (type != m_TypeCheck)
                SetColorByType();
            m_TypeCheck = type;
        }

        public void Init(PrototypingItemProps _Props)
        {
            type = _Props.Type;
            rectangle = gameObject.AddComponent<Rectangle>();
            rectangle.Width = 0.97f;
            rectangle.Height = 0.97f;
            rectangle.Type = Rectangle.RectangleType.RoundedSolid;
            rectangle.CornerRadius = 0.1f;
            SetColorByType();

            // if (_Props.Type == PrototypingItemType.NodeStart || _Props.Type == PrototypingItemType.Node)
            // {
            //     var go = new GameObject("Index");
            //     go.SetParent(gameObject);
            //     go.transform.localPosition = Vector3.zero;
            //     index = go.AddComponent<TextMeshPro>();
            //     index.alignment = TextAlignmentOptions.Center;
            //     index.fontSize = 6;
            //     index.text = _Props.Index.ToNumeric();
            //     index.color = Color.black;    
            // }
            
            m_Initialized = true;
        }

        private void SetColorByType()
        {
            rectangle.Color = ColorByType(type);
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
    }
}
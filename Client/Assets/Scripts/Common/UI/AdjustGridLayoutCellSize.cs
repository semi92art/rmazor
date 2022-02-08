using Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

//https://forum.unity.com/threads/solved-how-to-make-grid-layout-group-cell-size-x-auto-expand.448534/
namespace UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AdjustGridLayoutCellSize : MonoBehaviour
    {
        public enum Axis { X, Y }
        public enum RatioMode { Free, Fixed }
   
        [SerializeField] private Axis expand;
        [SerializeField] private RatioMode ratioMode;
        [SerializeField] private float cellRatio = 1;
 
        private RectTransform m_Transform;
        private GridLayoutGroup m_Grid;
 
        private void Awake()
        {
            m_Transform = (RectTransform)transform;
            m_Grid = GetComponent<GridLayoutGroup>();
        }
        
        private void Start()
        {
            UpdateCellSize();
        }
 
        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }
 
#if UNITY_EDITOR
        [ExecuteAlways]
        private void Update()
        {
            UpdateCellSize();
        }
#endif
 
        private void OnValidate()
        {
            m_Transform = (RectTransform)transform;
            m_Grid = GetComponent<GridLayoutGroup>();
            UpdateCellSize();
        }
 
        private void UpdateCellSize()
        {
            if (m_Grid.IsNull())
                return;
            var count = m_Grid.constraintCount;
            var padding = m_Grid.padding;
            if (expand == Axis.X)
            {
                float spacing = (count - 1) * m_Grid.spacing.x;
                float contentSize = m_Transform.rect.width - padding.left - padding.right - spacing;
                float sizePerCell = contentSize / count;
                m_Grid.cellSize = new Vector2(sizePerCell, ratioMode == RatioMode.Free ? m_Grid.cellSize.y : sizePerCell * cellRatio);
           
            }
            else
            {
                float spacing = (count - 1) * m_Grid.spacing.y;
                float contentSize = m_Transform.rect.height - padding.top - padding.bottom -spacing;
                float sizePerCell = contentSize / count;
                m_Grid.cellSize = new Vector2(ratioMode == RatioMode.Free ? m_Grid.cellSize.x : sizePerCell * cellRatio, sizePerCell);
            }
        }
    }
}
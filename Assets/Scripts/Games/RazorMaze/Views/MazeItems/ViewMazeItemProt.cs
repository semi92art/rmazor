using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    [ExecuteInEditMode, Serializable]
    public class ViewMazeItemProt : MonoBehaviour, IViewMazeItem
    {
        #region serialized fields
        
        [SerializeField] private ShapeRenderer shape;
        [SerializeField] private SpriteRenderer hint;
        [HideInInspector] public EMazeItemType typeCheck;
        [SerializeField] private ViewMazeItemProps props;
        [SerializeField, HideInInspector] private V2Int mazeSize;
        
        #endregion
        
        #region nonpublic members
        
        private static Color _blockCol = new Color(0.53f, 0.53f, 0.53f, 0.8f);
        private static Color _trapCol = new Color(1f, 0.29f, 0.29f);
        private static Color _turretCol = new Color(1f, 0.14f, 0.7f);
        private static Color _portalCol = new Color(0.13f, 1f, 0.07f);
        private static Color _blockTransfToNodeCol = new Color(0.17f, 0.2f, 1f);

        #endregion
        
        #region api

        public ViewMazeItemProps Props
        {
            get => props;
            set => props = value;
        }
        
        public void Init(ViewMazeItemProps _Props, V2Int _MazeSize)
        {
            mazeSize = _MazeSize;
            props = _Props;
            SetShape(_Props.Type, _Props.IsNode, _Props.IsStartNode);
        }

        public void SetLocalPosition(Vector2 _Position)
        {
            transform.localPosition = _Position;
        }

        public void SetLocalScale(float _Scale)
        {
            transform.localScale = _Scale * Vector3.one;
        }

        public bool Equal(MazeItem _MazeItem)
        {
            return _MazeItem.Path == props.Path && _MazeItem.Type == props.Type;
        }
        
        #endregion
        
        #region editor api

        public void SetType(EMazeItemType _Type, bool _IsNode, bool _IsStartNode)
        {
            props.Type = _Type;
            props.IsNode = _IsNode;
            props.IsStartNode = _IsStartNode;
            SetShape(_Type, _IsNode, _IsStartNode);
            typeCheck = _Type;
        }
        
        #endregion
        
        #region nonpublic methods

        private void SetShape(EMazeItemType _Type, bool _IsNode, bool _IsStartNode)
        {
            var converter = new CoordinateConverter();
            converter.Init(mazeSize);
            
            gameObject.DestroyChildrenSafe();
            transform.localPosition = converter.ToLocalMazeItemPosition(props.Position);
            var sh = gameObject.GetOrAddComponent<Rectangle>();
            sh.Width = 0.97f * converter.GetScale();
            sh.Height = 0.97f * converter.GetScale();
            sh.Type = Rectangle.RectangleType.RoundedSolid;
            sh.CornerRadius = 0.1f;
            sh.Color = GetShapeColor(props.Type, false, _IsNode, _IsStartNode);
            shape = sh;
            
            if (props.IsNode)
            {
                shape.SortingOrder = 0;
                return;
            }
            SetHintByType(props.Type);
            SetSortingOrderByType(_Type);
        }

        private void SetHintByType(EMazeItemType _Type)
        {
            var go = new GameObject("Hint");
            go.SetParent(gameObject);
            go.transform.SetLocalPosXY(Vector2.zero);
            hint = go.AddComponent<SpriteRenderer>();
            
            string objectName = null;
            switch (_Type)
            {
                case EMazeItemType.Block: break;
                case EMazeItemType.BlockTransformingToNode: objectName = "shredinger"; break;
                case EMazeItemType.Portal:                  objectName = "portal"; break;
                case EMazeItemType.Turret:                  objectName = "turret"; break;
                case EMazeItemType.TrapMoving:              objectName = "trap-moving"; break;
                case EMazeItemType.BlockMovingGravity:      objectName = "block-gravity"; break;
                case EMazeItemType.TrapMovingGravity:       objectName = "trap-gravity"; break;
                case EMazeItemType.TrapIncreasing:          objectName = "trap-increase"; break;
                case EMazeItemType.TurretRotating:          objectName = "turret-rotate"; break;
                case EMazeItemType.TrapReact:               objectName = "trap-react"; break;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }

            if (string.IsNullOrEmpty(objectName)) 
                return;
            var result = PrefabUtilsEx.GetObject<Sprite>("prot_icons", objectName);
            hint.sprite = result;
        }

        private void SetSortingOrderByType(EMazeItemType _Type)
        {
            int shapeSortingOrder;
            switch (_Type)
            {
                case EMazeItemType.TrapReact:
                case EMazeItemType.TrapMovingGravity:
                case EMazeItemType.BlockMovingGravity:
                    shapeSortingOrder = 1;
                    break;
                case EMazeItemType.Block: 
                    shapeSortingOrder = 2;
                    break;
                case EMazeItemType.Portal:
                case EMazeItemType.Turret:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving:
                case EMazeItemType.TurretRotating:
                case EMazeItemType.BlockTransformingToNode:
                    shapeSortingOrder = 12;
                    break;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
            shape.SortingOrder = shapeSortingOrder;
            if (!hint.IsNull())
            {
                hint.sortingOrder = shapeSortingOrder + 1;
            }
        }
        
        
        private static Color GetShapeColor(EMazeItemType _Type, bool _Inner, bool _IsNode, bool _IsStartNode)
        {
            if (_IsNode)
                return _IsStartNode ? new Color(1f, 0.93f, 0.51f) : Color.white;
            
            if (_Inner)
                return Color.black;

            switch (_Type)
            {
            
                case EMazeItemType.Block:                   
                case EMazeItemType.BlockMovingGravity: 
                    return _blockCol; 
                case EMazeItemType.BlockTransformingToNode:
                    return _blockTransfToNodeCol;
                case EMazeItemType.Portal: 
                    return _portalCol;
                case EMazeItemType.TrapReact:               
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving: 
                case EMazeItemType.TrapMovingGravity:
                    return _trapCol;
                case EMazeItemType.Turret: 
                case EMazeItemType.TurretRotating: 
                    return _turretCol;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying || props.IsNode)
                return;

            Gizmos.color = GetShapeColor(props.Type, false, false, false);
            
            switch (props.Type)
            {
                case EMazeItemType.Block:
                case EMazeItemType.BlockTransformingToNode:
                case EMazeItemType.TurretRotating:
                    //do nothing
                    break;
                case EMazeItemType.TrapIncreasing:
                    DrawGizmosTrapIncreasing();
                    break;
                case EMazeItemType.TrapMoving:
                case EMazeItemType.BlockMovingGravity:
                case EMazeItemType.TrapMovingGravity:
                    DrawGizmosPath();
                    break;
                case EMazeItemType.TrapReact:
                case EMazeItemType.Turret:
                    DrawGizmosDirections();
                    break;
                case EMazeItemType.Portal:
                    DrawGizmosPortal();
                    break;
                default: throw new SwitchCaseNotImplementedException(props.Type);
            }
        }

        private void DrawGizmosPath()
        {
            if (!props.Path.Any())
                return;
            for (int i = 0; i < props.Path.Count; i++)
            {
                var pos = props.Path[i];
                Gizmos.DrawSphere(ToWorldPosition(pos), 1);
                if (i == props.Path.Count - 1)
                    return;
                Gizmos.DrawLine(ToWorldPosition(pos), ToWorldPosition(props.Path[i + 1]));
            }
        }

        private void DrawGizmosDirections()
        {
            if (!props.Directions.Any())
                return;
            var pos = ToWorldPosition(props.Position);
            Gizmos.DrawSphere(pos, 1);
            foreach (var dir in props.Directions)
            {
                Gizmos.DrawSphere(ToWorldPosition(props.Position + dir), 1);
                Gizmos.DrawLine(pos, ToWorldPosition(props.Position + dir));
            }
        }

        private void DrawGizmosPortal()
        {
            if (props.Pair == default)
                return;
            var pos = ToWorldPosition(props.Position);
            var pairPos = ToWorldPosition(props.Pair);
            Gizmos.DrawSphere(pos, 1);
            Gizmos.DrawSphere(pairPos, 1);
            Gizmos.DrawLine(pos, pairPos);
        }

        private void DrawGizmosTrapIncreasing()
        {
            var p1 = new V2Int(props.Position.X - 1, props.Position.Y - 1);
            var p2 = new V2Int(props.Position.X - 1, props.Position.Y + 1);
            var p3 = new V2Int(props.Position.X + 1, props.Position.Y + 1);
            var p4 = new V2Int(props.Position.X + 1, props.Position.Y - 1);

            var p1_2 = (Vector3)ToWorldPosition(p1);
            var p2_2 = (Vector3)ToWorldPosition(p2);
            var p3_2 = (Vector3)ToWorldPosition(p3);
            var p4_2 = (Vector3)ToWorldPosition(p4);

            Gizmos.DrawLine(p1_2, p2_2);
            Gizmos.DrawLine(p2_2, p3_2);
            Gizmos.DrawLine(p3_2, p4_2);
            Gizmos.DrawLine(p4_2, p1_2);
            Gizmos.DrawLine(p1_2, p3_2);
            Gizmos.DrawLine(p2_2, p4_2);

            Gizmos.DrawSphere(p1_2, 1);
            Gizmos.DrawSphere(p2_2, 1);
            Gizmos.DrawSphere(p3_2, 1);
            Gizmos.DrawSphere(p4_2, 1);
        }

        private Vector2 ToWorldPosition(V2Int _Point)
        {
            var converter = new CoordinateConverter();
            converter.Init(mazeSize);
            return converter.ToLocalMazeItemPosition(_Point).PlusY(converter.GetCenter().y);
        }
        
        #endregion
    }
    
    [Serializable]
    public class ViewMazeItemProps
    {
        public bool IsNode;
        public bool IsStartNode;
        public EMazeItemType Type;
        public V2Int Position;
        public List<V2Int> Path = new List<V2Int>();
        public List<V2Int> Directions = new List<V2Int>();
        public V2Int Pair;
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    [ExecuteInEditMode, Serializable]
    public class ViewMazeItemProt : MonoBehaviour, IViewMazeItem
    {
        #region serialized fields
        
        [HideInInspector, SerializeField] public ShapeRenderer shape;
        [SerializeField] private SpriteRenderer hint;
        [HideInInspector] public EMazeItemType typeCheck;
        [SerializeField] private ViewMazeItemProps props;
        [SerializeField, HideInInspector] private V2Int mazeSize;
        
        #endregion
        
        #region nonpublic members

        private static Color _pathItemCol = Color.white;
        private static Color _pathStartItemCol = new Color(1f, 0.93f, 0.51f);
        private static Color _blockCol = new Color(0.53f, 0.53f, 0.53f, 0.8f);
        private static Color _trapCol = new Color(1f, 0.29f, 0.29f);
        private static Color _turretCol = new Color(1f, 0.14f, 0.7f);
        private static Color _portalCol = new Color(0.13f, 1f, 0.07f);
        private static Color _blockShredingerCol = new Color(0.49f, 0.79f, 1f);
        private static Color _springboardCol = new Color(0.41f, 1f, 0.79f);
        
        private bool m_Active;

        #endregion
        
        #region api

        public bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                if (props.Type == EMazeItemType.ShredingerBlock)
                {
                    shape.Color = m_Active ? _blockCol : _blockShredingerCol;
                    // hint.enabled = m_Active;
                }
            }
        }

        public ViewMazeItemProps Props
        {
            get => props;
            set => props = value;
        }
        
        public void Init(ViewMazeItemProps _Props, V2Int _MazeSize)
        {
            mazeSize = _MazeSize;
            props = _Props;
            SetShapeAndHint(
                _Props.Type, 
                _Props.IsNode,
                _Props.IsStartNode);
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
            typeCheck = _Type;
            props.Type = _Type;
            props.IsNode = _IsNode;
            props.IsStartNode = _IsStartNode;
            if (_Type == EMazeItemType.Springboard)
                props.Directions = new List<V2Int> {V2Int.up + V2Int.left};
            SetShapeAndHint(_Type, _IsNode, _IsStartNode);
        }

        public void SetSpringboardDirection(V2Int _Direction)
        {
            props.Directions = new List<V2Int>{_Direction};
            SetShapeAndHint(EMazeItemType.Springboard, false, false);
        }
        
        #endregion
        
        #region nonpublic methods

        private void SetShapeAndHint(EMazeItemType _Type, bool _IsNode, bool _IsStartNode)
        {
            var converter = new CoordinateConverter();
            converter.Init(mazeSize);
            
            gameObject.DestroyChildrenSafe();
            transform.SetLocalPosXY(converter.ToLocalMazeItemPosition(props.Position));

            SetShapeByType(_Type, _IsNode, _IsStartNode);
            if (props.IsNode)
            {
                shape.SortingOrder = 0;
                return;
            }
            SetHintByType(_Type, _IsNode);
            GetShapeSortingOrder(_Type, _IsNode);
        }

        private void SetShapeByType(EMazeItemType _Type, bool _IsNode, bool _IsStartNode)
        {
            var converter = new CoordinateConverter();
            converter.Init(mazeSize);
            var sh = gameObject.GetOrAddComponent<Rectangle>();
            sh.Width = 0.97f * converter.GetScale();
            sh.Height = 0.97f * converter.GetScale();
            sh.Type = Rectangle.RectangleType.RoundedSolid;
            sh.CornerRadius = 0.1f;
            shape = sh;
            shape.Color = GetShapeColor(_Type, false, _IsNode, _IsStartNode);
            shape.SortingOrder = GetShapeSortingOrder(_Type, _IsNode);
        }

        private void SetHintByType(EMazeItemType _Type, bool _IsNode)
        {
            if (_IsNode)
                return;
            var go = new GameObject("Hint");
            go.SetParent(gameObject);
            go.transform.SetLocalPosXY(Vector2.zero);
            hint = go.AddComponent<SpriteRenderer>();
            
            string objectName = null;
            switch (_Type)
            {
                case EMazeItemType.Block: break;
                case EMazeItemType.Springboard:             objectName = "springboard"; break;
                case EMazeItemType.ShredingerBlock:         objectName = "shredinger"; break;
                case EMazeItemType.Portal:                  objectName = "portal"; break;
                case EMazeItemType.Turret:                  objectName = "turret"; break;
                case EMazeItemType.TrapMoving:              objectName = "trap-moving"; break;
                case EMazeItemType.GravityBlock:            objectName = "block-gravity"; break;
                case EMazeItemType.GravityTrap:             objectName = "trap-gravity"; break;
                case EMazeItemType.TrapIncreasing:          objectName = "trap-increase"; break;
                case EMazeItemType.TurretRotating:          objectName = "turret-rotate"; break;
                case EMazeItemType.TrapReact:               objectName = "trap-react"; break;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }

            if (string.IsNullOrEmpty(objectName)) 
                return;
            hint.sprite = PrefabUtilsEx.GetObject<Sprite>("prot_icons", objectName);
            hint.sortingOrder = GetShapeSortingOrder(_Type, false) + 1;
        }

        private int GetShapeSortingOrder(EMazeItemType _Type, bool _IsNode)
        {
            if (_IsNode)
                return 0;
            int result;
            switch (_Type)
            {
                case EMazeItemType.TrapReact:
                case EMazeItemType.GravityTrap:
                case EMazeItemType.GravityBlock:
                    result = 1;
                    break;
                case EMazeItemType.Block: 
                    result = 2;
                    break;
                case EMazeItemType.Portal:
                case EMazeItemType.Turret:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving:
                case EMazeItemType.TurretRotating:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.Springboard:
                    result = 12;
                    break;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
            return result;
        }
        
        
        private static Color GetShapeColor(EMazeItemType _Type, bool _Inner, bool _IsNode, bool _IsStartNode)
        {
            if (_IsNode)
                return _IsStartNode ? _pathStartItemCol : _pathItemCol;
            
            if (_Inner)
                return Color.black;

            switch (_Type)
            {
                case EMazeItemType.Block:                   
                case EMazeItemType.GravityBlock: 
                    return _blockCol; 
                case EMazeItemType.ShredingerBlock:
                    return _blockShredingerCol;
                case EMazeItemType.Portal: 
                    return _portalCol;
                case EMazeItemType.TrapReact:               
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving: 
                case EMazeItemType.GravityTrap:
                    return _trapCol;
                case EMazeItemType.Turret: 
                case EMazeItemType.TurretRotating: 
                    return _turretCol;
                case EMazeItemType.Springboard:
                    return _springboardCol;
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
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.TurretRotating:
                    //do nothing
                    break;
                case EMazeItemType.TrapIncreasing:
                    DrawGizmosTrapIncreasing();
                    break;
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityTrap:
                    DrawGizmosPath();
                    break;
                case EMazeItemType.TrapReact:
                case EMazeItemType.Turret:
                    DrawGizmosDirections();
                    break;
                case EMazeItemType.Portal:
                    DrawGizmosPortal();
                    break;
                case EMazeItemType.Springboard:
                    DrawGizmosSpringboard();
                    break;
                default: throw new SwitchCaseNotImplementedException(props.Type);
            }
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
            var pairItem = LevelDesigner.Instance.maze
                .FirstOrDefault(_Item => _Item.props.Position == props.Pair);
            if (pairItem != null && (pairItem.props.Type != EMazeItemType.Portal || pairItem.props.IsNode))
            {
                props.Pair = default;
                return;
            }
            var pos = ToWorldPosition(props.Position);
            var pairPos = ToWorldPosition(props.Pair);
            Gizmos.DrawSphere(pos, 1);
            Gizmos.DrawSphere(pairPos, 1);
            Gizmos.DrawLine(pos, pairPos);
        }

        private void DrawGizmosSpringboard()
        {
            if (!props.Directions.Any())
                return;
            var pos = ToWorldPosition(props.Position);
            var dir = props.Directions.First();

            var dirs = new List<V2Int>();
            if (dir == V2Int.up + V2Int.left)
                dirs = new List<V2Int> {V2Int.up, V2Int.left};
            else if (dir == V2Int.up + V2Int.right)
                dirs = new List<V2Int> {V2Int.up, V2Int.right};
            else if (dir == V2Int.down + V2Int.left)
                dirs = new List<V2Int> {V2Int.down, V2Int.left};
            else if (dir == V2Int.down + V2Int.right)
                dirs = new List<V2Int> {V2Int.down, V2Int.right};
            
            foreach (var direct in dirs)
            {
                var posVector2 = props.Position.ToVector2();
                var dirVector2 = direct.ToVector2();
                Gizmos.DrawSphere(ToWorldPosition(posVector2 + dirVector2), 0.5f);
                Gizmos.DrawLine(pos, ToWorldPosition(posVector2 + dirVector2));
            }
        }
        
        private Vector2 ToWorldPosition(V2Int _Point)
        {
            var converter = new CoordinateConverter();
            converter.Init(mazeSize);
            return converter.ToLocalMazeItemPosition(_Point).PlusY(converter.GetCenter().y);
        }
        
        private Vector2 ToWorldPosition(Vector2 _Point)
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
        public List<V2Int> Directions = new List<V2Int>{V2Int.zero};
        public V2Int Pair;
    }
}
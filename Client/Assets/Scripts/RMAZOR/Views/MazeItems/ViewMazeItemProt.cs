using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using GameHelpers;
using Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    [ExecuteInEditMode, Serializable, SelectionBase]
    public class ViewMazeItemProt : MonoBehaviour, IViewMazeItem
    {
        #region serialized fields
        
        [SerializeField] private ViewMazeItemProps props;
        [SerializeField, HideInInspector] private ShapeRenderer shape;
        [SerializeField, HideInInspector] private SpriteRenderer hint;
        [SerializeField, HideInInspector] private V2Int mazeSize;
        
        #endregion

        #region nonpublic members

        private IMazeCoordinateConverter m_Converter;
        private IContainersGetter        m_ContainersGetter;
        private IPrefabSetManager        m_PrefabSetManager;
        private IMazeCoordinateConverter Converter
        {
            get
            {
                if (m_Converter == null || !m_Converter.InitializedAndMazeSizeSet())
                {
                    m_PrefabSetManager = new PrefabSetManager(new AssetBundleManagerFake());
                    var settings = m_PrefabSetManager.GetObject<ViewSettings>(
                        "model_settings", "view_settings");
                    m_Converter = new MazeCoordinateConverter(settings, null);
                    m_ContainersGetter = new ContainersGetterRmazor(null, m_Converter);
                    m_Converter.GetContainer = m_ContainersGetter.GetContainer;
                    m_Converter.Init();
                    m_Converter.MazeSize = mazeSize;
                }
                return m_Converter;
            }
        }

        #endregion
        
        #region api
        
        public V2Int MazeSize
        {
            set => mazeSize = value;
        }
        
        public ViewMazeItemProps Props
        {
            get => props;
            set => props = value;
        }
        
        public void Init(ViewMazeItemProps _Props)
        {
            props = _Props;
            SetShapeAndHint(_Props.Type, _Props.IsNode);
        }

        public void SetLocalPosition(Vector2 _Position)
        {
            transform.SetLocalPosXY(_Position);
        }

        public void SetLocalScale(float _Scale)
        {
            transform.localScale = _Scale * Vector3.one;
        }

        public bool Equal(IMazeItemProceedInfo _Info)
        {
            return Props != null && Props.Equals(_Info);
        }
        
        #endregion
        
        #region editor api

        public void SetType(EMazeItemType _Type, bool _IsNode, bool _IsStartNode)
        {
            props.Type = _Type;
            props.IsNode = _IsNode;
            props.IsStartNode = _IsStartNode;
            if (_Type == EMazeItemType.Springboard)
                props.Directions = new List<V2Int> {V2Int.Up + V2Int.Left};
            SetShapeAndHint(_Type, _IsNode);
        }

        public void SetSpringboardDirection(V2Int _Direction)
        {
            if (!props.Directions.Any())
                props.Directions.Add(_Direction);
            else
                props.Directions[0] = _Direction;
        }
        
        #endregion
        
        #region nonpublic methods

        private void SetShapeAndHint(EMazeItemType _Type, bool _IsNode)
        {
            gameObject.DestroyChildrenSafe();
            transform.SetLocalPosXY(Converter.ToLocalMazeItemPosition(props.Position));

            SetShapeByType(_Type, _IsNode);
            if (props.IsNode)
            {
                shape.SortingOrder = 0;
                return;
            }
            SetHintByType(_Type, _IsNode);
            GetShapeSortingOrder(_Type, _IsNode);
        }

        private void SetShapeByType(EMazeItemType _Type, bool _IsNode)
        {
            var sh = gameObject.GetOrAddComponent<Rectangle>();
            sh.Width = 0.97f * Converter.Scale;
            sh.Height = 0.97f * Converter.Scale;
            sh.Type = Rectangle.RectangleType.RoundedSolid;
            sh.CornerRadius = 0.1f;
            shape = sh;
            shape.Color = GetShapeColor(_Type, false, _IsNode);
            shape.SortingOrder = GetShapeSortingOrder(_Type, _IsNode);
        }

        private void SetHintByType(EMazeItemType _Type, bool _IsNode)
        {
            if (_IsNode)
                return;
            var hintGo = new GameObject("Hint");
            hintGo.SetParent(gameObject);
            hintGo.transform.SetLocalPosXY(Vector2.zero);
            hint = hintGo.AddComponent<SpriteRenderer>();
            hintGo.layer = 31;
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
                case EMazeItemType.GravityBlockFree:        objectName = "gravity-block-free"; break;
                case EMazeItemType.TrapReact:               objectName = "trap-react"; break;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
            if (string.IsNullOrEmpty(objectName)) 
                return;
            hint.sprite = m_PrefabSetManager.GetObject<Sprite>("prot_icons", objectName);
            hint.sortingOrder = GetShapeSortingOrder(_Type, false) + 1;
        }

        private int GetShapeSortingOrder(EMazeItemType _Type, bool _IsPath) => 
            _IsPath ? SortingOrders.Path : SortingOrders.GetBlockSortingOrder(_Type);


        private static Color GetShapeColor(EMazeItemType _Type, bool _Inner, bool _IsNode)
        {
            if (_IsNode)
                return Color.white;
            if (_Inner)
                return Color.black;
            switch (_Type)
            {
                case EMazeItemType.Block:                   
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityBlockFree:
                    return new Color(0.5f, 0.5f, 0.5f);
                case EMazeItemType.ShredingerBlock:
                    return new Color(0.49f, 0.79f, 1f);
                case EMazeItemType.Portal:
                    return new Color(0.13f, 1f, 0.07f);
                case EMazeItemType.TrapReact:               
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving: 
                case EMazeItemType.GravityTrap:
                    return new Color(1f, 0.29f, 0.29f);
                case EMazeItemType.Turret: 
                    return new Color(0.99f, 0.14f, 0.7f);
                case EMazeItemType.Springboard:
                    return new Color(0.41f, 1f, 0.79f);
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            if (props.Blank)
            {
                Gizmos.color = Color.magenta;
                DrawGizmosBlankNode();
            }
            if (props.IsNode)
            {
                if (!props.IsStartNode) 
                    return;
                Gizmos.color = Color.yellow;
                DrawGizmosStartNode();
                return;
            }
            Gizmos.color = GetShapeColor(props.Type, false, props.IsNode);
            switch (props.Type)
            {
                case EMazeItemType.Block:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.GravityBlockFree:
                case EMazeItemType.GravityTrap:
                    //do nothing
                    break;
                case EMazeItemType.TrapIncreasing:
                    DrawGizmosTrapIncreasing();
                    break;
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlock:
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

        private void DrawGizmosStartNode()
        {
            var p = (Vector3)ToWorldPosition(props.Position);
            Gizmos.DrawSphere(p, 1f);
        }
        
        private void DrawGizmosBlankNode()
        {
            var p = (Vector3)ToWorldPosition(props.Position);
            Gizmos.DrawCube(p, Vector3.one);
        }
        
        private void DrawGizmosTrapIncreasing()
        {
            var p1 = new V2Int(props.Position.X - 1, props.Position.Y - 1);
            var p2 = new V2Int(props.Position.X - 1, props.Position.Y + 1);
            var p3 = new V2Int(props.Position.X + 1, props.Position.Y + 1);
            var p4 = new V2Int(props.Position.X + 1, props.Position.Y - 1);

            var p1V2 = (Vector3)ToWorldPosition(p1);
            var p2V2 = (Vector3)ToWorldPosition(p2);
            var p3V2 = (Vector3)ToWorldPosition(p3);
            var p4V2 = (Vector3)ToWorldPosition(p4);

            Gizmos.DrawLine(p1V2, p2V2);
            Gizmos.DrawLine(p2V2, p3V2);
            Gizmos.DrawLine(p3V2, p4V2);
            Gizmos.DrawLine(p4V2, p1V2);
            Gizmos.DrawLine(p1V2, p3V2);
            Gizmos.DrawLine(p2V2, p4V2);

            Gizmos.DrawSphere(p1V2, 0.5f);
            Gizmos.DrawSphere(p2V2, 0.5f);
            Gizmos.DrawSphere(p3V2, 0.5f);
            Gizmos.DrawSphere(p4V2, 0.5f);
        }

        private void DrawGizmosPath()
        {
            if (!props.Path.Any())
                return;
            for (int i = 0; i < props.Path.Count; i++)
            {
                var pos = props.Path[i];
                Gizmos.DrawSphere(ToWorldPosition(pos), 0.5f);
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
            foreach (var dir in props.Directions)
            {
                Gizmos.DrawSphere(ToWorldPosition(props.Position + dir), 0.5f);
                Gizmos.DrawLine(pos, ToWorldPosition(props.Position + dir));
            }
        }
        
        private void DrawGizmosPortal()
        {
#if UNITY_EDITOR
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
            Gizmos.DrawSphere(pos, 0.5f);
            Gizmos.DrawSphere(pairPos, 0.5f);
            Gizmos.DrawLine(pos, pairPos);
#endif
        }

        private void DrawGizmosSpringboard()
        {
            if (!props.Directions.Any())
                return;
            var pos = ToWorldPosition(props.Position);
            var dir = props.Directions.First();

            var dirs = new List<V2Int>();
            if (dir == V2Int.Up + V2Int.Left)
                dirs = new List<V2Int> {V2Int.Up, V2Int.Left};
            else if (dir == V2Int.Up + V2Int.Right)
                dirs = new List<V2Int> {V2Int.Up, V2Int.Right};
            else if (dir == V2Int.Down + V2Int.Left)
                dirs = new List<V2Int> {V2Int.Down, V2Int.Left};
            else if (dir == V2Int.Down + V2Int.Right)
                dirs = new List<V2Int> {V2Int.Down, V2Int.Right};
            
            foreach (var direct in dirs)
            {
                Vector2 posVector2 = props.Position;
                Vector2 dirVector2 = direct;
                Gizmos.DrawSphere(ToWorldPosition(posVector2 + dirVector2), 0.3f);
                Gizmos.DrawLine(pos, ToWorldPosition(posVector2 + dirVector2));
            }
        }

        private Vector2 ToWorldPosition(Vector2 _Point)
        {
            return Converter.ToLocalMazeItemPosition(_Point);
        }
        
        #endregion

        #region unused api
        
        public EAppearingState  AppearingState       { get; set; }
        public Component[]      Shapes               => null;
        public EProceedingStage ProceedingStage      { get; set; }
        public bool             ActivatedInSpawnPool { get; set; }
        public GameObject       Object               => gameObject;
        
        public void Appear(bool _Appear) { }
        public object Clone() => new object();
        public void OnLevelStageChanged(LevelStageArgs _Args) { }

        #endregion
    }
}
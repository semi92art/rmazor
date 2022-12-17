using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    [ExecuteInEditMode, Serializable, SelectionBase]
    public partial class ViewMazeItemProt : MonoBehInitBase, IViewMazeItem
    {
        #region serialized fields

        [SerializeField] private ViewMazeItemProps props;
        [SerializeField] private float             scale;
        
        [SerializeField, HideInInspector] private ShapeRenderer                     shape;
        [SerializeField, HideInInspector] private SpriteRenderer                    hint;
        [SerializeField, HideInInspector] private V2Int                             mazeSize;
        [SerializeField, HideInInspector] private CoordinateConverterRmazorInEditor converter;
        
        #endregion

        #region nonpublic members

        private IContainersGetterRmazorInEditor m_ContainersGetter;
        private IPrefabSetManager m_PrefabSetManager;
        private CoordinateConverterRmazorInEditor Converter
        {
            get
            {
                if (converter == null || !converter.IsValid())
                {
                    m_PrefabSetManager = new PrefabSetManager(new AssetBundleManagerFake());
                    var settings = m_PrefabSetManager.GetObject<ViewSettings>(
                        "configs", "view_settings");
                    converter = CoordinateConverterRmazorInEditor.Create(settings, null, false);
                    m_ContainersGetter = new ContainersGetterRmazorInEditor(null, converter);
                    converter.GetContainer = m_ContainersGetter.GetContainer;
                    converter.Init();
                    converter.SetMazeSize(mazeSize);
                    scale = converter.Scale;
                }
                return converter;
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
        
        public void UpdateState(ViewMazeItemProps _Props)
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

        public void SetDirection(V2Int _Direction)
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
            hintGo.transform.SetLocalScaleXY(GetScale() * Vector2.one);
            hint = hintGo.AddComponent<SpriteRenderer>();
            hintGo.layer = 31;
            string objectName = _Type switch
            {
                EMazeItemType.Block            => null,
                EMazeItemType.Springboard      => "springboard",
                EMazeItemType.ShredingerBlock  => "shredinger",
                EMazeItemType.Portal           => "portal",
                EMazeItemType.Turret           => "turret",
                EMazeItemType.TrapMoving       => "trap-moving",
                EMazeItemType.GravityBlock     => "block-gravity",
                EMazeItemType.GravityTrap      => "trap-gravity",
                EMazeItemType.TrapIncreasing   => "trap-increase",
                EMazeItemType.GravityBlockFree => "gravity-block-free",
                EMazeItemType.TrapReact        => "trap-react",
                EMazeItemType.Hammer           => "hammer",
                EMazeItemType.Spear            => "spear",
                EMazeItemType.Diode            => "diode",
                EMazeItemType.KeyLock          => "key-lock",
                _                              => throw new SwitchExpressionException(_Type)
            };
            if (string.IsNullOrEmpty(objectName)) 
                return;
            m_PrefabSetManager ??= new PrefabSetManager(new AssetBundleManagerFake());
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
            var blockCol = new Color(0.5f, 0.5f, 0.5f);
            var trapCol = new Color(1f, 0.29f, 0.29f);
            return _Type switch
            {
                EMazeItemType.Block            => blockCol,
                EMazeItemType.GravityBlock     => blockCol,
                EMazeItemType.GravityBlockFree => blockCol,
                EMazeItemType.TrapReact        => trapCol,
                EMazeItemType.TrapIncreasing   => trapCol,
                EMazeItemType.TrapMoving       => trapCol,
                EMazeItemType.GravityTrap      => trapCol,
                EMazeItemType.ShredingerBlock  => new Color(0.49f, 0.79f, 1f),
                EMazeItemType.Portal           => new Color(0.13f, 1f, 0.07f),
                EMazeItemType.Turret           => new Color(0.99f, 0.14f, 0.7f),
                EMazeItemType.Springboard      => new Color(0.41f, 1f, 0.79f),
                EMazeItemType.Hammer           => new Color(0.66f, 0.43f, 0.12f),
                EMazeItemType.Spear            => new Color(0.7f, 0.67f, 1f),
                EMazeItemType.Diode            => new Color(0.63f, 1f, 0.7f),
                EMazeItemType.KeyLock          => new Color(1f, 0.67f, 0.96f),
                _                              => throw new SwitchExpressionException(_Type)
            };
        }

        #endregion

        #region unused api
        
        public Component[]      Renderers               => null;
        public EAppearingState  AppearingState       { get; set; }
        public EProceedingStage ProceedingStage      { get; set; }
        public bool             ActivatedInSpawnPool { get; set; }
        public GameObject       Object               => gameObject;
        
        public void Appear(bool _Appear) { }
        public object Clone() => new object();
        public void OnLevelStageChanged(LevelStageArgs _Args) { }

        #endregion
    }
}
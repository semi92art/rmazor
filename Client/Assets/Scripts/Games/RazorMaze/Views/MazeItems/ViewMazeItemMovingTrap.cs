using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems.Props;
using Games.RazorMaze.Views.Utils;
using Ticker;
using TimeProviders;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap : ViewMazeItemBase, IViewMazeItemMovingTrap, IUpdateTick
    {
        #region nonpublic members
        
        private bool m_Rotate;
        private Vector2 m_Position;
        
        #endregion
        
        #region shapes

        protected override object[] Shapes => new[] {m_Saw};
        private SpriteRenderer m_Saw;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemMovingTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTimeProvider _GameTimeProvider,
            IGameTicker _GameTicker) 
            : base(
                _ViewSettings,
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTimeProvider,
                _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemMovingTrap(
            ViewSettings,
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTimeProvider,
            GameTicker);
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                if (!value)
                    m_Saw.enabled = false;
            }
        }

        public override bool Proceeding
        {
            get => m_Rotate;
            set
            {
                if (value) StartRotation();
                else StopRotation();
            }
        }

        public override void Init(ViewMazeItemProps _Props)
        {
            base.Init(_Props);
            Proceeding = true;
        }
        
        public void OnMoveStarted(MazeItemMoveEventArgs _Args) { }

        public void OnMoving(MazeItemMoveEventArgs _Args)
        {
            var precisePosition = Vector2.Lerp(
                _Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(precisePosition));
        }

        public void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
        }

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            if (!m_Rotate)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            Object.transform.Rotate(Vector3.forward * rotSpeed);
        }
        
        #endregion

        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;
            var saw = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<SpriteRenderer>(
                    "Moving Trap", 
                    ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            saw.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "moving_trap");
            saw.color = DrawingUtils.ColorTrap;
            saw.sortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            saw.enabled = false;
            var coll = go.AddComponent<CircleCollider2D>();
            coll.radius = 0.5f;

            go.transform.localScale = Vector3.one * CoordinateConverter.GetScale();
            
            Object = go;
            m_Saw = saw;
        }

        private void StartRotation()
        {
            m_Rotate = true;
        }

        private void StopRotation()
        {
            m_Rotate = false;
        }
        #endregion
    }
}
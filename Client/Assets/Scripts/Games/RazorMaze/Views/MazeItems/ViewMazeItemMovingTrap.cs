using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using UnityEngine;
using UnityGameLoopDI;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap : ViewMazeItemBase, IViewMazeItemMovingTrap, IUpdateTick
    {

        #region nonpublic members

        private SpriteRenderer m_Saw;
        private bool m_Rotate;
        
        #endregion
        
        #region inject
        
        private ViewSettings ViewSettings { get; }
        
        public ViewMazeItemMovingTrap(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ITicker _Ticker,
            ViewSettings _ViewSettings) 
            : base(_CoordinateConverter, _ContainersGetter, _Ticker)
        {
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api

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

        public void OnMoveStarted(MazeItemMoveEventArgs _Args)
        {
        }

        public void OnMoving(MazeItemMoveEventArgs _Args)
        {
            var pos = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(pos));
        }

        public void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
        }

        public void UpdateTick()
        {
            if (!m_Rotate)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            Object.transform.Rotate(Vector3.forward * rotSpeed);
        }

        public object Clone() => new ViewMazeItemMovingTrap(
            CoordinateConverter, ContainersGetter, Ticker, ViewSettings);
        
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
            saw.color = ViewUtils.ColorTrap;
            saw.sortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);
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
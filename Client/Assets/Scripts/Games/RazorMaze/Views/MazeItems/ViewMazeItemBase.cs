using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public abstract class ViewMazeItemBase
    {
        #region nonpublic members
        
        protected bool m_Activated;
        
        #endregion

        #region inject

        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ITicker Ticker { get; }
        

        protected ViewMazeItemBase (
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ITicker _Ticker)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            Ticker = _Ticker;
        }

        #endregion
        
        #region api
        
        public ViewMazeItemProps Props { get; set; }
        
        public virtual bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                if (Object.activeSelf != value)
                    Object.SetActive(value);
            }
        }
        
        public virtual GameObject Object { get; protected set; }
        public virtual bool Proceeding { get; set; }
        

        public virtual void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
            Ticker.Register(this);
        }

        public bool Equal(MazeItem _MazeItem)
        {
            return _MazeItem.Path == Props.Path && _MazeItem.Type == Props.Type;
        }
        
        public void SetLocalPosition(Vector2 _Position)
        {
            Object.transform.SetLocalPosXY(_Position);
        }

        public void SetLocalScale(float _Scale)
        {
            Object.transform.localScale = _Scale * Vector3.one;
        }
        
        #endregion
        
        #region nonpublic methods
        
        protected abstract void SetShape();
        
        #endregion
    }
}
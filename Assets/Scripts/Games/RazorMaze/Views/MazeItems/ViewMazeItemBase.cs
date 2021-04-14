using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using UnityEngine;
using UnityGameLoopDI;

namespace Games.RazorMaze.Views.MazeItems
{
    public abstract class ViewMazeItemBase : Ticker
    {
        public ViewMazeItemProps Props { get; set; }
        
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }

        protected ViewMazeItemBase (ICoordinateConverter _CoordinateConverter, IContainersGetter _ContainersGetter)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
        }

        public virtual GameObject Object { get; protected set; }
        
        public virtual bool Active { get; set; }

        public virtual void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
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

        protected abstract void SetShape();
    }
}
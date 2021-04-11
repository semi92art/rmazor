using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public abstract class ViewMazeItemBase
    {
        public ViewMazeItemProps Props { get; set; }
        
        protected Transform Item;

        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }

        protected ViewMazeItemBase (ICoordinateConverter _CoordinateConverter, IContainersGetter _ContainersGetter)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
        }
        
        public virtual bool Active { get; set; }

        public bool Equal(MazeItem _MazeItem)
        {
            return _MazeItem.Path == Props.Path && _MazeItem.Type == Props.Type;
        }
        
        public void SetLocalPosition(Vector2 _Position)
        {
            Item.transform.SetLocalPosXY(_Position);
        }

        public void SetLocalScale(float _Scale)
        {
            Item.transform.localScale = _Scale * Vector3.one;
        }
    }
}
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using TimeProviders;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public abstract class ViewMazeItemBase : IViewMazeItem
    {
        #region nonpublic members
        
        protected bool Initialized { get; set; }
        protected bool m_Activated;
        private bool m_Proceeding;
        
        #endregion

        #region inject

        protected ViewSettings ViewSettings { get; }
        protected IModelMazeData Data { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected IGameTimeProvider GameTimeProvider { get; }
        protected ITicker Ticker { get; }
        

        protected ViewMazeItemBase (
            ViewSettings _ViewSettings,
            IModelMazeData _Data,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTimeProvider _GameTimeProvider,
            ITicker _Ticker)
        {
            ViewSettings = _ViewSettings;
            Data = _Data;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTimeProvider = _GameTimeProvider;
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

        public virtual bool Proceeding
        {
            get => m_Proceeding;
            set => m_Proceeding = value;
        }
        
        public abstract object Clone();

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                Appear(true);
            else if (_Args.Stage == ELevelStage.Unloaded)
                Appear(false);
        }
        

        public virtual void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
            Ticker.Register(this);
            Initialized = true;
        }

        public bool Equal(MazeItem _MazeItem)
        {
            if (Props == null)
                return false;
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
        protected abstract void Appear(bool _Appear);


        #endregion
    }
}
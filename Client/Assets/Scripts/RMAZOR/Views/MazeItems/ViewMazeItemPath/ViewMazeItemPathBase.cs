using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPath :
        IViewMazeItem,
        ICharacterMoveStarted, 
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        event UnityAction MoneyItemCollected;
        void              Collect(bool _Collect, bool _OnStart);
    }
    
    public abstract class ViewMazeItemPathBase : ViewMazeItemBase, IViewMazeItemPath
    {
        #region inject
        
        protected IViewMazeItemPathItem      PathItem  { get; }
        protected IViewMazeItemsPathInformer Informer  { get; }

        protected ViewMazeItemPathBase(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewMazeItemPathItem          _PathItem,
            IViewMazeItemsPathInformer  _Informer)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder)
        {
            PathItem = _PathItem;
            Informer = _Informer;
        }

        #endregion

        #region api
        
        public event UnityAction MoneyItemCollected;
        
        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_ActivatedInSpawnPool = value;
                EnableInitializedShapes(false);
            }
        }

        public override void Init()
        {
            if (Initialized)
                return;
            base.Init();
        }

        public abstract void Collect(bool _Collect, bool _OnStart);
        
        public abstract void OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args);
        public abstract void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args);
        public abstract void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args);

        #endregion

        #region nonpublic methods

        protected abstract void EnableInitializedShapes(bool _Enable);

        protected void RaiseMoneyItemCollected()
        {
            MoneyItemCollected?.Invoke();
        }

        #endregion

    }
}
using Common;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common.ViewMazeMoneyItems;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPath :
        IViewMazeItem,
        ICharacterMoveStarted, 
        ICharacterMoveContinued
    {
        event UnityAction MoneyItemCollected;
        void              Collect(bool _Collect, bool _OnStart = false);
    }
    
    public abstract class ViewMazeItemPathBase : ViewMazeItemBase, IViewMazeItemPath
    {
        #region inject
        
        protected IViewMazeMoneyItem         MoneyItem { get; }
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
            IViewMazeMoneyItem          _MoneyItem,
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
            MoneyItem = _MoneyItem;
            Informer  = _Informer;
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

        public abstract void Collect(bool _Collect, bool _OnStart = false);
        public abstract void OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args);
        public abstract void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args);

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
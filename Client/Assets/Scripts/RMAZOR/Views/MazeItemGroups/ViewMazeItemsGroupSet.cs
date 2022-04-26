using Common;
using Common.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeItemsGroupSet :
        IOnLevelStageChanged,
        ICharacterMoveStarted, 
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IInit
    {
        IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        IViewMazeTurretsGroup          TurretsGroup          { get; }
        IViewMazePortalsGroup          PortalsGroup          { get; }
        IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        IViewMazeHammersGroup          HammersGroup          { get; }
        IViewMazeBazookasGroup        BazookasGroup        { get; }
        
        
        IViewMazeItemGroup[] GetGroups();
    }

    public class ViewMazeItemsGroupSet : InitBase, IViewMazeItemsGroupSet
    {
        #region nonpublic members

        private IViewMazeItemGroup[] m_GroupsCached;

        #endregion

        #region inject

        public IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        public IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        public IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        public IViewMazeTurretsGroup          TurretsGroup          { get; }
        public IViewMazePortalsGroup          PortalsGroup          { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        public IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        public IViewMazeHammersGroup          HammersGroup          { get; }
        public IViewMazeBazookasGroup        BazookasGroup        { get; }

        public ViewMazeItemsGroupSet(
            IViewMazeMovingItemsGroup      _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup  _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup    _TrapsIncItemsGroup,
            IViewMazeTurretsGroup          _TurretsGroup,
            IViewMazePortalsGroup          _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup,
            IViewMazeGravityItemsGroup     _GravityItemsGroup,
            IViewMazeHammersGroup          _HammersGroup,
            IViewMazeBazookasGroup        _BazookasGroup)
        {
            MovingItemsGroup      = _MovingItemsGroup;
            TrapsReactItemsGroup  = _TrapsReactItemsGroup;
            TrapsIncItemsGroup    = _TrapsIncItemsGroup;
            TurretsGroup          = _TurretsGroup;
            PortalsGroup          = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
            GravityItemsGroup     = _GravityItemsGroup;
            HammersGroup          = _HammersGroup;
            BazookasGroup        = _BazookasGroup;
        }

        #endregion

        #region api

        public IViewMazeItemGroup[] GetGroups()
        {
            if (m_GroupsCached != null)
                return m_GroupsCached;
            m_GroupsCached = new IViewMazeItemGroup[]
            {
                MovingItemsGroup,
                TrapsReactItemsGroup,
                TrapsIncItemsGroup,
                TurretsGroup,
                PortalsGroup,
                ShredingerBlocksGroup,
                SpringboardItemsGroup,
                GravityItemsGroup,
                HammersGroup,
                BazookasGroup
            };
            return m_GroupsCached;
        }

        public override void Init()
        {
            var proceeders = GetInterfaceOfProceeders<IInit>();
            foreach (var proceeder in proceeders)
                proceeder?.Init();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var groups = GetGroups();
            foreach (var g in groups)
                g.OnLevelStageChanged(_Args);
        }

        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveContinued(_Args);
        }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveFinished(_Args);
        }

        #endregion

        #region nonpublic methods

        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return System.Array.ConvertAll(GetGroups(), _Item => _Item as T);
        }

        #endregion
        
    }
}
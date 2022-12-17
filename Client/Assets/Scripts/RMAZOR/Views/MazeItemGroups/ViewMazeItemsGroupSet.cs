using Common;
using Common.Helpers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;

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
        IViewMazeSpringboardsGroup     SpringboardsGroup     { get; }
        IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        IViewMazeHammersGroup          HammersGroup          { get; }
        IViewMazeSpearsGroup           SpearsGroup           { get; }
        IViewMazeDiodesGroup           DiodesGroup           { get; }
        IViewMazeKeyLockGroup          KeyLockGroup          { get; }
        
        
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
        public IViewMazeSpringboardsGroup     SpringboardsGroup     { get; }
        public IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        public IViewMazeHammersGroup          HammersGroup          { get; }
        public IViewMazeSpearsGroup           SpearsGroup           { get; }
        public IViewMazeDiodesGroup           DiodesGroup           { get; }
        public IViewMazeKeyLockGroup          KeyLockGroup          { get; }

        private ViewMazeItemsGroupSet(
            IViewMazeMovingItemsGroup      _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup  _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup    _TrapsIncItemsGroup,
            IViewMazeTurretsGroup          _TurretsGroup,
            IViewMazePortalsGroup          _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardsGroup     _SpringboardsGroup,
            IViewMazeGravityItemsGroup     _GravityItemsGroup,
            IViewMazeHammersGroup          _HammersGroup,
            IViewMazeSpearsGroup           _SpearsGroup,
            IViewMazeDiodesGroup           _DiodesGroup,
            IViewMazeKeyLockGroup          _KeyLockGroup)
        {
            MovingItemsGroup      = _MovingItemsGroup;
            TrapsReactItemsGroup  = _TrapsReactItemsGroup;
            TrapsIncItemsGroup    = _TrapsIncItemsGroup;
            TurretsGroup          = _TurretsGroup;
            PortalsGroup          = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardsGroup     = _SpringboardsGroup;
            GravityItemsGroup     = _GravityItemsGroup;
            HammersGroup          = _HammersGroup;
            SpearsGroup           = _SpearsGroup;
            DiodesGroup           = _DiodesGroup;
            KeyLockGroup          = _KeyLockGroup;
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
                SpringboardsGroup,
                GravityItemsGroup,
                HammersGroup,
                SpearsGroup,
                DiodesGroup,
                KeyLockGroup
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
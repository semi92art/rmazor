using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeItemsGroupSet :
        IOnLevelStageChanged,
        ICharacterMoveStarted, 
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        IViewMazeTurretsGroup          TurretsGroup          { get; }
        IViewMazePortalsGroup          PortalsGroup          { get; }
        IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        
        IViewMazeItemGroup[] GetGroups();
    }

    public class ViewMazeItemsGroupSet : IViewMazeItemsGroupSet
    {
        #region nonpublic members

        private IViewMazeItemGroup[] m_GroupsCached;

        #endregion
        
        public  IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        public  IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        public  IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        public  IViewMazeTurretsGroup          TurretsGroup          { get; }
        public  IViewMazePortalsGroup          PortalsGroup          { get; }
        public  IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public  IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        public  IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }

        public ViewMazeItemsGroupSet(
            IViewMazeMovingItemsGroup      _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup  _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup    _TrapsIncItemsGroup,
            IViewMazeTurretsGroup          _TurretsGroup,
            IViewMazePortalsGroup          _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup, 
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup, 
            IViewMazeGravityItemsGroup     _GravityItemsGroup)
        {
            MovingItemsGroup      = _MovingItemsGroup;
            TrapsReactItemsGroup  = _TrapsReactItemsGroup;
            TrapsIncItemsGroup    = _TrapsIncItemsGroup;
            TurretsGroup          = _TurretsGroup;
            PortalsGroup          = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
            GravityItemsGroup     = _GravityItemsGroup;
        }
        
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
                GravityItemsGroup
            };
            return m_GroupsCached;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var groups = GetGroups();
            foreach (var g in groups)
                g.OnLevelStageChanged(_Args);
        }

        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs     _Args)
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

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs   _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveFinished(_Args);
        }
        
        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return System.Array.ConvertAll(m_GroupsCached, _Item => _Item as T);
        }
    }
}
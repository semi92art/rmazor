using System.Linq;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;

namespace RMAZOR.Models
{
    public interface IModelItemsProceedersSet :
        IGetAllProceedInfos,
        IOnLevelStageChanged,
        ICharacterMoveStarted, 
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        ITrapsMovingProceeder      TrapsMovingProceeder      { get; }
        IGravityItemsProceeder     GravityItemsProceeder     { get; }
        ITrapsReactProceeder       TrapsReactProceeder       { get; }
        ITrapsIncreasingProceeder  TrapsIncreasingProceeder  { get; }
        ITurretsProceeder          TurretsProceeder          { get; }
        IPortalsProceeder          PortalsProceeder          { get; }
        IShredingerBlocksProceeder ShredingerBlocksProceeder { get; }
        ISpringboardProceeder      SpringboardProceeder      { get; }
        IHammersProceeder          HammersProceeder          { get; }
        IBazookasProceeder        BazookasProceeder        { get; }
        
        IItemsProceeder[] GetProceeders();
    }
    
    public class ModelItemsProceedersSet : IModelItemsProceedersSet
    {
        #region nonpublic members

        private IItemsProceeder[] m_ProceedersCached;

        #endregion

        #region inject
        
        public ITrapsMovingProceeder      TrapsMovingProceeder      { get; }
        public IGravityItemsProceeder     GravityItemsProceeder     { get; }
        public ITrapsReactProceeder       TrapsReactProceeder       { get; }
        public ITrapsIncreasingProceeder  TrapsIncreasingProceeder  { get; }
        public ITurretsProceeder          TurretsProceeder          { get; }
        public IPortalsProceeder          PortalsProceeder          { get; }
        public IShredingerBlocksProceeder ShredingerBlocksProceeder { get; }
        public ISpringboardProceeder      SpringboardProceeder      { get; }
        public IHammersProceeder          HammersProceeder          { get; }
        public IBazookasProceeder        BazookasProceeder        { get; }


        public ModelItemsProceedersSet(
            ITrapsMovingProceeder      _TrapsMovingProceeder,
            IGravityItemsProceeder     _GravityItemsProceeder,
            ITrapsReactProceeder       _TrapsReactProceeder,
            ITrapsIncreasingProceeder  _TrapsIncreasingProceeder,
            ITurretsProceeder          _TurretsProceeder,
            IPortalsProceeder          _PortalsProceeder,
            IShredingerBlocksProceeder _ShredingerBlocksProceeder,
            ISpringboardProceeder      _SpringboardProceeder,
            IHammersProceeder          _HammersProceeder,
            IBazookasProceeder        _BazookasProceeder)
        {
            TrapsMovingProceeder      = _TrapsMovingProceeder;
            GravityItemsProceeder     = _GravityItemsProceeder;
            TrapsReactProceeder       = _TrapsReactProceeder;
            TrapsIncreasingProceeder  = _TrapsIncreasingProceeder;
            TurretsProceeder          = _TurretsProceeder;
            PortalsProceeder          = _PortalsProceeder;
            ShredingerBlocksProceeder = _ShredingerBlocksProceeder;
            SpringboardProceeder      = _SpringboardProceeder;
            HammersProceeder          = _HammersProceeder;
            BazookasProceeder        = _BazookasProceeder;
        }

        #endregion

        #region api

        public System.Func<IMazeItemProceedInfo[]> GetAllProceedInfos
        {
            set
            {
                foreach (var item in GetInterfaceOfProceeders<IGetAllProceedInfos>()
                    .Where(_Item => _Item != null))
                {
                    item.GetAllProceedInfos = value;
                }
            }
        }
        
        public IItemsProceeder[] GetProceeders()
        {
            if (m_ProceedersCached != null)
                return m_ProceedersCached;
            m_ProceedersCached = new IItemsProceeder[]
            {
                TrapsMovingProceeder,
                GravityItemsProceeder,
                TrapsReactProceeder,
                TrapsIncreasingProceeder,
                TurretsProceeder,
                PortalsProceeder,
                ShredingerBlocksProceeder,
                SpringboardProceeder,
                HammersProceeder,
                BazookasProceeder,
            };
            return m_ProceedersCached;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var groups = GetProceeders();
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
            return System.Array.ConvertAll(GetProceeders(), _Item => _Item as T);
        }

        #endregion
    }
}
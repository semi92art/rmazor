using System.Collections.Generic;
using System.Linq;

namespace Common.Network.DataFieldFilters
{
    public abstract class DataFieldFilterBase
    {

        #region nonpublic members

        protected readonly int AccountId;
        protected readonly ushort[] FieldIds;
        protected IGameClient GameClient { get; }
        
        #endregion
        
        #region api
        
        public bool OnlyLocal { get; set; }
        
        #endregion

        #region constructors

        protected DataFieldFilterBase(IGameClient _GameClient, int _AccountId, params ushort[] _FieldIds)
        {
            GameClient = _GameClient;
            AccountId = _AccountId;    
            FieldIds = _FieldIds;
        }

        #endregion
        
        #region nonpublic methods
        
        protected bool WasFiltered<TSource>(IEnumerable<TSource> _Fields, bool _ForceRefresh)
        {
            return _Fields != null && _Fields.Any() && !_ForceRefresh;
        }

        #endregion
    }
}
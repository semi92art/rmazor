using System.Collections.Generic;
using System.Linq;

namespace GameHelpers
{
    public abstract class DataFieldFilterBase
    {
        #region nonpublic members

        protected readonly int AccountId;
        protected readonly ushort[] FieldIds;
        
        #endregion
        
        #region api
        
        public bool OnlyLocal { get; set; }
        
        #endregion

        #region constructors

        protected DataFieldFilterBase(int _AccountId, params ushort[] _FieldIds)
        {
            AccountId = _AccountId;    
            FieldIds = _FieldIds;
        }

        #endregion
        
        #region nonpublic methods
        
        protected bool WasFiltered<TSource>(IEnumerable<TSource> _Fields, bool _ForceRefresh) =>
            _Fields != null && _Fields.Any() && !_ForceRefresh;
        
        #endregion
    }
}
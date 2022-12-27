using System.Collections.Generic;
using System.Linq;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Network.DataFieldFilters;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public class RemoteConfigPropertyInfo
    {
        #region nonpublic members
        
        [JsonIgnore] private readonly GameDataFieldFilter m_Filter;

        #endregion

        #region ctor
        
        public RemoteConfigPropertyInfo(
            GameDataFieldFilter _Filter,
            System.Type         _Type,
            string              _Key,
            UnityAction<object> _SetProperty,
            bool                _IsJson = false)
        {
            m_Filter         = _Filter;
            Type             = _Type;
            Key              = _Key;
            IsJson           = _IsJson;
            SetPropertyValue = _SetProperty;
        }

        #endregion

        #region api
        
        [JsonProperty] public string      Key    { get; }
        [JsonProperty] public System.Type Type   { get; }
        [JsonProperty] public bool        IsJson { get; }

        
        [JsonIgnore] public UnityAction<object> SetPropertyValue { get; }

        [JsonIgnore]
        public Entity<object> GetCachedValueEntity
        {
            get
            {
                var entity = new Entity<object>();
                m_Filter.Filter(_Fields =>
                {
                    var field = GetField(_Fields, Key);
                    if (field?.GetValue() == null)
                    {
                        entity.Result = EEntityResult.Fail;
                        return;
                    }

                    entity.Value = field.GetValue();
                    entity.Result = EEntityResult.Success;
                });
                return entity;
            }
        }

        public void SetCachedValue(object _Value)
        {
            m_Filter.Filter(_Fields =>
            {
                var field = GetField(_Fields, Key);
                if (field == null)
                    return;
                field.SetValue(_Value).Save(true);
            });
        }

        #endregion

        #region nonpublic methods
        
        private static GameDataField GetField(IEnumerable<GameDataField> _Fields, string _FieldName)
        {
            ushort id = (ushort)MazorCommonUtils.StringToHash(_FieldName);
            return _Fields.FirstOrDefault(_F => _F.FieldId == id);
        }

        #endregion
    }
}
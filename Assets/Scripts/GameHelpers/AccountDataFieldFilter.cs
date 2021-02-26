using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entities;
using Network;
using Network.Packets;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace GameHelpers
{
    public class AccountDataFieldFilter : DataFieldFilterBase
    {
        #region nonpublic members
        
        private List<AccountDataField> m_Fields;
        
        #endregion
        
        #region constructors

        public AccountDataFieldFilter(int? _AccountId, params ushort[] _FieldIds)
            : base (_AccountId, _FieldIds) { }

        #endregion
        
        #region api

        public void Filter(UnityAction<IReadOnlyList<AccountDataField>> _FinishAction, bool _ForceRefresh = false)
        {
            Coroutines.Run(FilterAccountFields(_FinishAction, _ForceRefresh));
        }

        public IReadOnlyList<AccountDataField> Filter(bool _ForceRefresh = false)
        {
            return FilterAccountFields(_ForceRefresh);
        }
        
        #endregion

        #region nonpublic methods

        private IEnumerator FilterAccountFields(
            UnityAction<IReadOnlyList<AccountDataField>> _FinishAction,
            bool _ForceRefresh)
        {
            if (WasFiltered(m_Fields, _ForceRefresh))
            {
                _FinishAction?.Invoke(m_Fields);
                yield break;
            }

            if (OnlyLocal)
            {
                m_Fields = GetCachedFields();
                _FinishAction?.Invoke(m_Fields);
                yield break;
            }
            
            var packet = CreatePacket();
            packet.OnSuccess(() =>
            {
                var dataFields = GetFromDtos(packet.Response.ToList());
                m_Fields = dataFields;
                foreach (var field in m_Fields)
                    field.Save(true);
                _FinishAction?.Invoke(dataFields);
            }).OnFail(() =>
            {
                if (AccountId != GameClientUtils.AccountId)
                    return;
                m_Fields = GetCachedFields();
                m_Fields.ForEach(_Field => _Field.Save(true));
                _FinishAction?.Invoke(m_Fields);
            });
            GameClient.Instance.Send(packet);
        }
        
        private IReadOnlyList<AccountDataField> FilterAccountFields(bool _ForceRefresh)
        {
            if (WasFiltered(m_Fields, _ForceRefresh))
                return m_Fields;
            
            if (AccountId == GameClientUtils.AccountId || OnlyLocal)
                m_Fields = GetCachedFields();
            else
            {
                var packet = CreatePacket();
                GameClient.Instance.Send(packet, false);
                m_Fields = NetworkUtils.IsPacketFail(packet.ResponseCode) ?
                    GetCachedFields() : GetFromDtos(packet.Response.ToList());
                m_Fields.ForEach(_Field => _Field.Save(true));
            }
            return m_Fields;
        }

        private List<AccountDataField> GetCachedFields()
        {
            return FieldIds
                .Select(_FieldId =>
                    SaveUtils.GetValue<AccountDataField>(
                        SaveKey.AccountDataFieldValue(AccountId, _FieldId)))
                .ToList();
        }

        private List<AccountDataField> GetFromDtos(IEnumerable<AccountFieldDto> _Dtos)
        {
            return _Dtos
                .Select(_DfvArgs => new AccountDataField(_DfvArgs))
                .ToList();
        }

        private AccountDataFieldsGetPacket CreatePacket()
        {
            var accFieldRequestDtos = CreateRequestFields();
            var args = new AccountFieldListDtoLite
            {
                DataFields = accFieldRequestDtos,
                Pagination = new PaginationDto()
            };
            return new AccountDataFieldsGetPacket(args);
        }
        
        private List<AccountFieldDtoLite> CreateRequestFields()
        {
            return FieldIds
                .Select(_FieldId => new AccountFieldDtoLite 
                    {AccountId = AccountId, FieldId = _FieldId})
                .ToList();
        }
        
        #endregion
    }
}
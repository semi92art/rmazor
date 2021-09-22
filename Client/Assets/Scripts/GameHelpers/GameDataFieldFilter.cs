using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Network;
using Network.Packets;
using UnityEngine.Events;
using Utils;

namespace GameHelpers
{
    public class GameDataFieldFilter : DataFieldFilterBase
    {
        #region nonpublic members

        private readonly int m_GameId;
        private List<GameDataField> m_Fields;
        
        #endregion
        
        #region constructors

        public GameDataFieldFilter(int _AccountId, int _GameId, params ushort[] _FieldIds)
            : base(_AccountId, _FieldIds)
        {
            m_GameId = _GameId;
        }

        #endregion
        
        #region api

        public void Filter(UnityAction<IReadOnlyList<GameDataField>> _FinishAction, bool _ForceRefresh = false)
        {
            Coroutines.Run(FilterGameFields(_FinishAction, _ForceRefresh));
        }

        public IReadOnlyList<GameDataField> Filter(bool _ForceRefresh = false)
        {
            return FilterGameFields(_ForceRefresh);
        }
        
        #endregion
        
        #region nonpublic methods

        private IEnumerator FilterGameFields(
            UnityAction<IReadOnlyList<GameDataField>> _FinishAction,
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
                m_Fields.ForEach(_Field => _Field.Save(true));
                _FinishAction?.Invoke(dataFields);
            }).OnFail(() =>
            {
                if (AccountId != GameClientUtils.AccountId)
                    return;
                m_Fields = GetCachedFields();
                _FinishAction?.Invoke(m_Fields);
            });
            GameClient.Instance.Send(packet);
        }
        
        private IReadOnlyList<GameDataField> FilterGameFields(bool _ForceRefresh)
        {
            if (WasFiltered(m_Fields, _ForceRefresh))
                return m_Fields;
            
            if (AccountId == GameClientUtils.AccountId || OnlyLocal)
                m_Fields = GetCachedFields();
            else
            {
                var packet = CreatePacket();
                GameClient.Instance.Send(packet, false);

                var dataFieldValues = GetFromDtos(packet.Response.ToList());
                m_Fields = dataFieldValues;
                m_Fields.ForEach(_Field => _Field.Save(true));
            }
            return m_Fields;
        }

        private List<GameDataField> GetCachedFields()
        {
            return FieldIds
                .Select(_FieldId =>
                    SaveUtils.GetValue<GameDataField>(
                        SaveKey.GameDataFieldValue(AccountId, m_GameId, _FieldId)))
                .ToList();
        }

        private List<GameDataField> GetFromDtos(IEnumerable<GameFieldDto> _Dtos)
        {
            return _Dtos
                .Select(_Dto => new GameDataField(_Dto))
                .ToList();
        }

        private GameDataFieldsGetPacket CreatePacket()
        {
            var accFieldRequestDtos = CreateRequestFields();
            var args = new GameFieldListDtoLite
            {
                DataFields = accFieldRequestDtos,
                Pagination = new PaginationDto()
            };
            return new GameDataFieldsGetPacket(args);
        }
        
        private List<GameFieldDtoLite> CreateRequestFields()
        {
            return FieldIds
                .Select(_FieldId => new GameFieldDtoLite 
                    {AccountId = AccountId, GameId = m_GameId, FieldId = _FieldId})
                .ToList();
        }
        
        #endregion
    }
}
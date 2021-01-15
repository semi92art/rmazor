using System;
using Network;
using Network.Packets;
using Utils;

namespace Entities
{
    public class GameDataField : DataFieldBase
    {
        private readonly int m_GameId;

        public GameDataField() { }
        
        public GameDataField(GameFieldDto _Args) 
            : base(_Args.AccountId, _Args.FieldId, _Args.Value, _Args.LastUpdate)
        {
            m_GameId = _Args.GameId;
        }

        public GameDataField(object _Value, int _AccountId, int _GameId, ushort _FieldId, DateTime _LastUpdate)
            : base(_AccountId, _FieldId, _Value, _LastUpdate)
        {
            m_GameId = _GameId;
        }
        
        public GameDataField(object _Value, int _AccountId, int _GameId, ushort _FieldId)
        : this(_Value, _AccountId, _GameId, _FieldId, DateTime.Now) { }

        public GameDataField SetValue(object _Value)
        {
            Value = _Value;
            return this;
        }

        public GameDataField Save(bool _OnlyLocal = false)
        {
            IsSaving = true;
            SaveUtils.PutValue(SaveKey.GameDataFieldValue(AccountId, m_GameId, FieldId), this);

            if (_OnlyLocal)
                return this;
            
            var newLastUpdate = DateTime.Now;
            var packetArgs = new[]
            {
                new GameFieldDto
                {
                    Value = Value,
                    AccountId = AccountId,
                    FieldId = FieldId,
                    GameId = m_GameId,
                    LastUpdate = newLastUpdate
                }
            };
            var packet = new GameDataFieldsSetPacket(packetArgs);
            packet.OnSuccess(() =>
                {
                    IsSaving = false;
                    LastUpdate = newLastUpdate;
                })
                .OnFail(() => IsSaving = false)
                .OnCancel(() => IsSaving = false);
            GameClient.Instance.Send(packet);
            return this;
        }
    }
}
using System;
using Network;
using Network.Packets;
using Utils;

namespace Entities
{
    public class AccountDataField : DataFieldBase
    {
        public AccountDataField() { }
        
        public AccountDataField(AccountFieldDto _Args)
            : base (_Args.AccountId, _Args.FieldId, _Args.Value, _Args.LastUpdate) { }

        public AccountDataField(object _Value, int _AccountId, ushort _FieldId, DateTime _LastUpdate)
            : base (_AccountId, _FieldId, _Value, _LastUpdate) { }
        
        public AccountDataField(object _Value, int _AccountId, ushort _FieldId)
            : this(_Value, _AccountId, _FieldId, DateTime.Now) { }
        
        public AccountDataField SetValue(object _Value)
        {
            Value = _Value;
            return this;
        }
        
        public AccountDataField Save(bool _OnlyLocal = false)
        {
            IsSaving = true;
            SaveUtils.PutValue(SaveKey.AccountDataFieldValue(AccountId, FieldId), this);

            if (_OnlyLocal)
                return this;

            var newLastUpdate = DateTime.Now;
            var packetArgs = new[]
            {
                new AccountFieldDto
                {
                    Value = Value,
                    AccountId = AccountId,
                    FieldId = FieldId,
                    LastUpdate = newLastUpdate
                }
            };
            var packet = new AccountDataFieldsSetPacket(packetArgs);
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
using System;
using System.Linq;
using Network;
using Network.Packets;
using Newtonsoft.Json;
using Utils;

namespace Entities
{
    public class GameDataField : DataFieldBase
    {
        [JsonIgnore] private readonly int m_GameId;
        [JsonIgnore] private bool m_IsSaving;

        [JsonIgnore]
        public override bool IsSaving
        {
            get => m_IsSaving;
            protected set
            {
                m_IsSaving = value;
                if (value)
                    GameClient.Instance.ExecutingGameFields.Add(this);
                else GameClient.Instance.ExecutingGameFields.Remove(this);
            }
        }


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

        public void Save(bool _OnlyLocal = false)
        {
            if (_OnlyLocal)
                IsSaving = true;
            SaveUtils.PutValue(SaveKey.GameDataFieldValue(AccountId, m_GameId, FieldId), this);

            if (_OnlyLocal)
            {
                IsSaving = false;
                return;
            }

            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                var execFields = GameClient.Instance.ExecutingGameFields;
                return execFields.Any(_F => _F.AccountId == AccountId
                                            && _F.FieldId == FieldId && _F.IsSaving);
            }, () =>
            {
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
                    .OnFail(() => IsSaving = false);
                IsSaving = true;
                GameClient.Instance.Send(packet);
            }));
        }
    }
}
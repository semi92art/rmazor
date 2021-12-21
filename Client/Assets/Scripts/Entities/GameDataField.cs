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
        [JsonIgnore] public  int  GameId { get; set; }
        [JsonIgnore] private bool m_IsSaving;

        [JsonIgnore]
        public override bool IsSaving
        {
            get => m_IsSaving;
            protected set
            {
                m_IsSaving = value;
                if (value)
                    GameClient.ExecutingGameFields.Add(this);
                else GameClient.ExecutingGameFields.Remove(this);
            }
        }


        public GameDataField() { }
        
        public GameDataField(IGameClient _GameClient, GameFieldDto _Args) 
            : base(_GameClient, _Args.AccountId, _Args.FieldId, _Args.Value, _Args.LastUpdate)
        {
            GameId = _Args.GameId;
        }

        public GameDataField(
            IGameClient _GameClient,
            object _Value, 
            int _AccountId,
            int _GameId,
            ushort _FieldId, 
            DateTime _LastUpdate)
            : base(_GameClient, _AccountId, _FieldId, _Value, _LastUpdate)
        {
            GameId = _GameId;
        }
        
        public GameDataField(
            IGameClient _GameClient,
            object _Value, 
            int _AccountId,
            int _GameId,
            ushort _FieldId)
        : this(_GameClient, _Value, _AccountId, _GameId, _FieldId, DateTime.Now) { }

        public GameDataField SetValue(object _Value)
        {
            Value = _Value;
            return this;
        }

        public void Save(bool _OnlyLocal = false)
        {
            SaveUtils.PutValue(SaveKeys.GameDataFieldValue(AccountId, GameId, FieldId), this);
            if (_OnlyLocal)
                return;
            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                var execFields = GameClient.ExecutingGameFields;
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
                        GameId = GameId,
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
                GameClient.Send(packet);
            }));
        }
    }
}
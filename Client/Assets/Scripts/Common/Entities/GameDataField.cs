using System;
using Common.Network;
using Common.Network.Packets;
using Common.Utils;
using Newtonsoft.Json;

namespace Common.Entities
{
    public class GameDataField : DataFieldBase
    {
        [JsonIgnore] public  int  GameId { get; set; }
        
        public GameDataField(IGameClient _GameClient, GameFieldDto _Args) 
            : base(_GameClient, _Args.AccountId, _Args.FieldId, _Args.Value, _Args.LastUpdate)
        {
            GameId = _Args.GameId;
        }

        // ReSharper disable once UnusedMember.Global
        public GameDataField() { }

        // ReSharper disable once MemberCanBePrivate.Global
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
            LastUpdate = DateTime.Now;
            SaveUtils.PutValue(SaveKeysCommon.GameDataFieldValue(AccountId, GameId, FieldId), this);
            if (_OnlyLocal)
                return;
            var packetArgs = new[]
            {
                new GameFieldDto
                {
                    Value = Value,
                    AccountId = AccountId,
                    FieldId = FieldId,
                    GameId = GameId,
                    LastUpdate = LastUpdate
                }
            };
            var packet = new GameDataFieldsSetPacket(packetArgs);
            GameClient.Send(packet);
        }
    }
}
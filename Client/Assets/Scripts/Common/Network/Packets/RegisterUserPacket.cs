﻿using Common.Utils;

namespace Common.Network.Packets
{
    public sealed class RegisterUserPacket : PacketBase
    {
        public override string Id => nameof(RegisterUserPacket);
        public override string Url => $"{GameClientUtils.ServerApiUrl}/api/accounts/register";
        public RegisterUserPacketResponseArgs Response { get; private set; }
        

        public RegisterUserPacket(RegisterUserPacketRequestArgs _Request) : base(_Request) { }

        public override void DeserializeResponse(string _Json)
        {
            if (NetworkUtils.IsPacketSuccess(ResponseCode))
                Response = Deserialize<RegisterUserPacketResponseArgs>(_Json);
            base.DeserializeResponse(_Json);
        }
    }
    
    public class RegisterUserPacketRequestArgs : LoginUserPacketRequestArgs
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int GameId { get; set; }
    }
    
    public class RegisterUserPacketResponseArgs : Account { }
}
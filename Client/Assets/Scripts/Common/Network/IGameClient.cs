using System.Collections.Generic;
using Common.Entities;

namespace Common.Network
{
    public interface IGameClient : IInit
    {
        List<GameDataField> ExecutingGameFields { get; }
        void                Send(IPacket _Packet, bool _Async = true);
    }
}
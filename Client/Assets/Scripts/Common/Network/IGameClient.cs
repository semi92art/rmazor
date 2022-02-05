using System.Collections.Generic;
using Common.Entities;

namespace Common.Network
{
    public interface IGameClient : IInit
    {
        void                Send(IPacket _Packet, bool _Async = true);
    }
}
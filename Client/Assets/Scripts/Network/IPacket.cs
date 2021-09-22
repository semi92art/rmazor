using System;

namespace Network
{
    public interface IOnPacketFinish
    {
        IPacket OnSuccess(Action _Action);
        IPacket OnFail(Action _Action);
    }
    
    
    public interface IPacket : IOnPacketFinish
    {
        string Id { get; }
        object Request { get; }
        string Url { get; }
        string ResponseRaw { get; }
        long ResponseCode { get; set; }
        string Method { get;}
        bool IsDone { get; }
        ErrorResponseArgs ErrorMessage { get; }
        void DeserializeResponse(string _Json);
        
    }
}
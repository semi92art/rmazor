using Network.PacketArgs;

namespace Network
{
    public interface IPacket
    {
        int Id { get; }
        object Request { get; }
        string Url { get; }
        string ResponseRaw { get; }
        long ResponseCode { get; set; }
        string Method { get;}
        bool OnlyOne { get; }
        bool IsDone { get; }
        ErrorResponseArgs ErrorMessage { get; }
        void DeserializeResponse(string _Json);
        IPacket OnSuccess(System.Action _Action);
        IPacket OnFail(System.Action _Action);
        IPacket OnCancel(System.Action _Action);
    }
}
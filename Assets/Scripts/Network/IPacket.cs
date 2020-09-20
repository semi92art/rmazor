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
        void DeserializeResponse(string _Json);
        void InvokeSuccess();
        void InvokeFail();
        void InvokeCancel();
    }
}
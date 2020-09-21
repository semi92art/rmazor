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
        void DeserializeResponse(string _Json);
    }
}
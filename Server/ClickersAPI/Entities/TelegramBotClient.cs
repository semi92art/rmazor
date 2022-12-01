namespace ClickersAPI.Entities
{
    public class BotClient : IId
    {
        public long ChatId { get; set; }
        public int  Id     { get; set; }
    }
}
namespace ClickersAPI.Helpers
{
    public class ErrorResponse
    {
        public int Id { get; }
        public string Message { get; }
        public object Content { get; }

        public ErrorResponse(int _Id, string _Message, object _Content = null)
        {
            Id = _Id;
            Message = _Message;
            Content = _Content;
        }
    }
}

namespace Network
{
    public class ErrorResponseArgs
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}; Message: {Message}";
        }
    }
}
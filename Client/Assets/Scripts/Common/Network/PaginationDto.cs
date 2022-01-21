namespace Common.Network
{
    public class PaginationDto
    {
        public int Page { get; set; } = 1;
        public int RecordsPerPage { get; set; } = 10;
    }
}
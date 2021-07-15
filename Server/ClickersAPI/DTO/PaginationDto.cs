namespace ClickersAPI.DTO
{
    public class PaginationDto
    {
        public int Page { get; set; } = 1;

        private int m_RecordsPerPage = 10;
        private readonly int m_MaxRecordsPerPage = 50;

        public int RecordsPerPage
        {
            get => m_RecordsPerPage;
            set => m_RecordsPerPage = value > m_MaxRecordsPerPage ? m_MaxRecordsPerPage : value;
        }
    }
}

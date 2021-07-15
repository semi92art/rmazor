using System.Collections.Generic;

namespace ClickersAPI.DTO
{
    public class AccountFieldListDtoLite
    {
        public List<AccountFieldDtoLite> DataFields { get; set; }
        public PaginationDto Pagination { get; set; }
    }
    
    public class GameFieldListDtoLite
    {
        public List<GameFieldDtoLite> DataFields { get; set; }
        public PaginationDto Pagination { get; set; }
    }
}
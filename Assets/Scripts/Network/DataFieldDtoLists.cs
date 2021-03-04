using System.Collections.Generic;

namespace Network
{
    public class GameFieldListDtoLite
    {
        public List<GameFieldDtoLite> DataFields { get; set; }
        public PaginationDto Pagination { get; set; }
    }
}
using System.Collections.Generic;

namespace Common.Network
{
    public class GameFieldListDtoLite
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public List<GameFieldDtoLite> DataFields { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public PaginationDto Pagination { get; set; }
    }
}
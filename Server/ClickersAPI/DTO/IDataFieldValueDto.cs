using ClickersAPI.Entities;

namespace ClickersAPI.DTO
{
    public interface IDataFieldValueDto : ILastUpdate
    {
        object Value { get; set; }
    }
}
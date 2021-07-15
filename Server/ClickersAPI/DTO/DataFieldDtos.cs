using ClickersAPI.Entities;

namespace ClickersAPI.DTO
{
    public class AccountFieldDtoLite : IAccountId, IFieldId
    {
        public int AccountId { get; set; }
        public ushort FieldId { get; set; }
    }
    
    public class GameFieldDtoLite : AccountFieldDtoLite, IGameId
    {
        public int GameId { get; set; }
    }
    
    public class AccountFieldDto : AccountFieldDtoLite, IDataFieldValueDto
    {
        public object Value { get; set; }
        public System.DateTime LastUpdate { get; set; }

        public AccountFieldDto()
        {
            LastUpdate = System.DateTime.Now;
        }
    }
    
    public class GameFieldDto : GameFieldDtoLite, IDataFieldValueDto
    {
        public object Value { get; set; }
        public System.DateTime LastUpdate { get; set; }

        public GameFieldDto()
        {
            LastUpdate = System.DateTime.Now;
        }
    }
}
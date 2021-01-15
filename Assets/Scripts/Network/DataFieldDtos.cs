namespace Network
{
    public class AccountFieldDtoLite
    {
        public int AccountId { get; set; }
        public ushort FieldId { get; set; }
    }
    
    public class GameFieldDtoLite : AccountFieldDtoLite
    {
        public int GameId { get; set; }
    }
    
    public class AccountFieldDto : AccountFieldDtoLite
    {
        public object Value { get; set; }
        public System.DateTime LastUpdate { get; set; }

        public AccountFieldDto()
        {
            LastUpdate = System.DateTime.Now;
        }
    }
    
    public class GameFieldDto : GameFieldDtoLite
    {
        public object Value { get; set; }
        public System.DateTime LastUpdate { get; set; }

        public GameFieldDto()
        {
            LastUpdate = System.DateTime.Now;
        }
    }
}
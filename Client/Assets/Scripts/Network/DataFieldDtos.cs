namespace Network
{
    public class GameFieldDtoLite
    {
        public int AccountId { get; set; }
        public ushort FieldId { get; set; }
        public int GameId { get; set; }
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
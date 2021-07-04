namespace ClickersAPI.Entities
{
    public class GameDataField : IId, IAccountId, IFieldId, IDataFieldValue, IGameId
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public ushort FieldId { get; set; }
        public System.DateTime LastUpdate { get; set; }
        
        public long? NumericValue { get; set; }
        public string StringValue { get; set; }
        public bool? BoolValue { get; set; }
        public decimal? FloatingValue { get; set; }
        public System.DateTime? DateTimeValue { get; set; }
        public int GameId { get; set; }
    }
}
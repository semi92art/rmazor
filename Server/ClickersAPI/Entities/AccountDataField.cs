namespace ClickersAPI.Entities
{
    public class AccountDataField : IId, IAccountId, IFieldId, IDataFieldValue
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
    }
}
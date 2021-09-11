namespace ClickersAPI.Entities
{
    public interface IDataFieldValue : ILastUpdate
    {
        long? NumericValue { get; set; }
        string StringValue { get; set; }
        bool? BoolValue { get; set; }
        decimal? FloatingValue { get; set; }
        System.DateTime? DateTimeValue { get; set; }
    }
}
namespace Common.Entities
{
    public enum EEntityResult
    {
        Pending,
        Success,
        Fail
    }

    public interface IEntity<T>
    {
        T             Value  { get; set; }
        EEntityResult Result { get; }
    }
    
    public class Entity<T> : IEntity<T>
    {
        public T             Value  { get; set; }
        public EEntityResult Result { get; set; } = EEntityResult.Pending;
        public string        Error  { get; set; }
    }
}
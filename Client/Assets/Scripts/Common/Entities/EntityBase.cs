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
    
    public abstract class EntityBase<T>
    {
        public T             Value  { get; set; }
        public EEntityResult Result { get; set; }
    }
}
namespace Entities
{
    public class SaveKey<T>
    {
        public bool   IsDirty     { get; set; } = true;
        public T      CachedValue { get; set; }
        public string Key         { get; }
    
        public SaveKey(string _Key)
        {
            Key = _Key;
        }
    }
}
namespace Common.Entities
{
    public class SaveKey<T>
    {
        public bool   WasSet     { get; set; }
        public T      CachedValue { get; set; }
        public string Key         { get; }
    
        public SaveKey(string _Key)
        {
            Key = _Key;
        }
    }
}
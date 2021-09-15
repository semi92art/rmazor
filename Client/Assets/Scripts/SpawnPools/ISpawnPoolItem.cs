namespace SpawnPools
{
    public interface IActivated
    {
        bool Activated { get; set; }
    }
    
    public interface ISpawnPoolItem : IActivated { }
}
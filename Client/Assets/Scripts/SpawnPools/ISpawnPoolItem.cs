namespace SpawnPools
{
    public interface IActivated
    {
        bool Activated { get; set; }
    }

    public interface ISpawnPoolItem
    {
        bool ActivatedInSpawnPool { get; set; }
    }
}
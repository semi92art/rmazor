namespace DI
{
    public class DiObject
    {
        protected DiObject()
        {
            ContainersManager.Instance.RegisterObject(this);
        }
    }
}
namespace DI
{
    public class ContainerObject
    {
        public ContainerObject()
        {
            ContainersManager.Instance.RegisterObject(this);
        }
    }
}
namespace Entities
{
    public interface IGameObserver
    {
        void OnNotify(string _NotifyMessage, params object[] _Args);
    }
}
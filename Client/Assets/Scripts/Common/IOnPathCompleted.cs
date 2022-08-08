using Common.Entities;

namespace Common
{
    public interface IOnPathCompleted
    {
        void OnPathCompleted(V2Int _LastPath);
    }
}
using Common.Entities;
using mazing.common.Runtime.Entities;

namespace Common
{
    public interface IOnPathCompleted
    {
        void OnPathCompleted(V2Int _LastPath);
    }
}
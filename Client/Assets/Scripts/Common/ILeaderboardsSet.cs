using System.Collections.Generic;

namespace Common
{
    public interface ILeaderboardsSet
    {
        Dictionary<ushort, string> GetSet();
    }
}
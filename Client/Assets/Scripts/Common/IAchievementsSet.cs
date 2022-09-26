using System.Collections.Generic;

namespace Common
{
    public interface IAchievementsSet
    {
        Dictionary<ushort, string> GetSet();
    }
}
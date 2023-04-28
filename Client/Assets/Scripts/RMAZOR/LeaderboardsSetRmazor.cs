using System.Collections.Generic;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.Utils;
using UnityEngine;

namespace RMAZOR
{
    public class LeaderboardsSetRmazor : ILeaderboardsSet
    {
        public Dictionary<ushort, string> GetSet()
        {
            string leaderboardId = CommonUtils.Platform switch
            {
                RuntimePlatform.Android      => "CgkI1IvonNkDEAIQBg",
                RuntimePlatform.IPhonePlayer => "level",
                RuntimePlatform.WebGLPlayer  => "levels",
                _                            => null
            };
            return new Dictionary<ushort, string> {{DataFieldIds.Level, leaderboardId}};
        }
    }
}
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Utils;
using UnityEngine;

namespace RMAZOR
{
    public class LeaderboardsSetRmazor : ILeaderboardsSet
    {
        public Dictionary<ushort, string> GetSet()
        {
            bool ios = CommonUtils.Platform == RuntimePlatform.IPhonePlayer;
            return new Dictionary<ushort, string>
            {
                { DataFieldIds.Level, ios ? "level" : "CgkI1IvonNkDEAIQBg"}
            };
        }
    }
}
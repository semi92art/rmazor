using System.Collections.Generic;
using Common;
using Common.Utils;
using UnityEngine;
using static Common.Managers.Achievements.AchievementKeys;

namespace RMAZOR
{
    public class AchievementsSetRmazor : IAchievementsSet
    {
        public Dictionary<ushort, string> GetSet()
        {
            bool ios = CommonUtils.Platform == RuntimePlatform.IPhonePlayer;
            return new Dictionary<ushort, string>
            {
                {Level10Finished, ios ? "level_0010_finished" : "CgkI1IvonNkDEAIQBQ"},
                {Level25Finished, ios ? "level_0025_finished" : "CgkI1IvonNkDEAIQEg"},
                {Level50Finished, ios ? "level_0050_finished" : "CgkI1IvonNkDEAIQCA"},
                {Level100Finished, ios ? "level_0100_finished" : "CgkI1IvonNkDEAIQBw"},
                {Level200Finished, ios ? "level_0200_finished" : "CgkI1IvonNkDEAIQCQ"},
                {Level300Finished, ios ? "level_0300_finished" : "CgkI1IvonNkDEAIQCg"},
                {Level400Finished, ios ? "level_0400_finished" : "CgkI1IvonNkDEAIQCw"},
                {Level500Finished, ios ? "level_0500_finished" : "CgkI1IvonNkDEAIQDA"},
                {Level600Finished, ios ? "level_0600_finished" : "CgkI1IvonNkDEAIQDQ"},
                {Level700Finished, ios ? "level_0700_finished" : "CgkI1IvonNkDEAIQDg"},
                {Level800Finished, ios ? "level_0800_finished" : "CgkI1IvonNkDEAIQDw"},
                {Level900Finished, ios ? "level_0900_finished" : "CgkI1IvonNkDEAIQEA"},
                {Level1000Finished, ios ? "level_1000_finished" : "CgkI1IvonNkDEAIQEQ"},
            };
        }
    }
}
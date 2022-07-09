using UnityEngine.Analytics;

namespace Common.Managers.Advertising
{
    public class AdProviderInfo
    {
        public string Source { get; set; }
        public float  ShowRate           { get; set; }
        public bool   Enabled            { get; set; }
    }
}
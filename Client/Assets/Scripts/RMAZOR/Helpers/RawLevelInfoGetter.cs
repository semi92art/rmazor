using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RMAZOR.Helpers
{
    public interface IRawLevelInfoGetter
    {
        Tuple<float, float, float> GetStarTimeRecords(string _LevelInfoRaw);
    }
    
    public class RawLevelInfoGetter : IRawLevelInfoGetter
    {
        private readonly StringBuilder m_StringBuilder         = new StringBuilder();
        private readonly CultureInfo   m_FloatValueCultureInfo = new CultureInfo("en-US");
        
        public Tuple<float, float, float> GetStarTimeRecords(string _LevelInfoRaw)
        {
            float v1 = GetFloatValue(_LevelInfoRaw, "T1");
            float v2 = GetFloatValue(_LevelInfoRaw, "T2");
            float v3 = GetFloatValue(_LevelInfoRaw, "T3");
            return new Tuple<float, float, float>(v1, v2, v3);
        }

        private float GetFloatValue(string _LevelInfoRaw, string _Key)
        {
            int startIdx = _LevelInfoRaw.IndexOf(_Key, StringComparison.InvariantCulture);
            if (startIdx == -1)
                return default;
            int idx = startIdx + _Key.Length;
            while (!char.IsDigit(_LevelInfoRaw[idx]))
            {
                idx++;
            }
            m_StringBuilder.Clear();
            while(char.IsDigit(_LevelInfoRaw[idx]))
            {
                m_StringBuilder.Append(_LevelInfoRaw[idx]);
                idx++;
            }
            idx++;
            m_StringBuilder.Append('.');
            while(char.IsDigit(_LevelInfoRaw[idx]))
            {
                m_StringBuilder.Append(_LevelInfoRaw[idx]);
                idx++;
            }
            IEnumerable<char> arr = m_StringBuilder.ToString().ToCharArray();
            string valS = new string(arr.ToArray());
            float.TryParse(valS, NumberStyles.Float, m_FloatValueCultureInfo, out float val);
            return val;
        }
    }
}
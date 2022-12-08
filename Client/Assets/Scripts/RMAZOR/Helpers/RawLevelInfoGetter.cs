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
        private readonly StringBuilder m_StringBuilder = new StringBuilder();
        
        public Tuple<float, float, float> GetStarTimeRecords(string _LevelInfoRaw)
        {
            int i1 = _LevelInfoRaw.IndexOf("T1", StringComparison.InvariantCulture);
            int i2 = _LevelInfoRaw.IndexOf("T2", StringComparison.InvariantCulture);
            int i3 = _LevelInfoRaw.IndexOf("T3", StringComparison.InvariantCulture);
            float v1 = GetFloatValue(_LevelInfoRaw, i1);
            float v2 = GetFloatValue(_LevelInfoRaw, i2);
            float v3 = GetFloatValue(_LevelInfoRaw, i3);
            return new Tuple<float, float, float>(v1, v2, v3);
        }

        private float GetFloatValue(string _LevelInfoRaw, int _StartIdx)
        {
            if (_StartIdx == -1)
                return default;
            int idx = _StartIdx;
            do
            {
                idx++;
            }
            while (!char.IsDigit(_LevelInfoRaw[idx]));
            
            while(char.IsDigit(_LevelInfoRaw[idx]))
            {
                idx++;
                m_StringBuilder.Append(_LevelInfoRaw[idx]);
            }
            IEnumerable<char> arr = m_StringBuilder.ToString().ToCharArray();
            arr = arr.Reverse();
            string valS = arr.ToString();
            float val = float.Parse(valS, NumberStyles.Float);
            return val;
        }
    }
}
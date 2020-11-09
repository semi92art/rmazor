using System;
using System.Collections.Generic;

namespace MkeyFW
{
    public class WheelRandomNumberGenerator
    {
        private Dictionary<int, double> m_DistribDict = new Dictionary<int, double>();
        private Random m_RandomGenerator = new Random();
        private double m_DistrSum;

        public WheelRandomNumberGenerator(Dictionary<int, double> _Distributions)
        {
            AddNumbers(_Distributions);
        }
    
        public int GetDistributedRandomNumber()
        {
            double rand = m_RandomGenerator.NextDouble();
            double ratio = 1.0f / m_DistrSum;
            double tempDistr = 0;
            foreach (int key in m_DistribDict.Keys)
            {
                tempDistr += m_DistribDict[key];
                if (rand / ratio <= tempDistr)
                    return key;
            }

            return 0;
        }
    
        private void AddNumbers(Dictionary<int, double> _Distributions)
        {
            foreach (var kvp in _Distributions)
                AddNumber(kvp.Key, kvp.Value);
        }
    
        private void AddNumber(int _Value, double _Distribution)
        {
            if (m_DistribDict.ContainsKey(_Value))
            {
                m_DistrSum -= m_DistribDict[_Value];
                m_DistribDict[_Value] = _Distribution;
            }
            else
                m_DistribDict.Add(_Value, _Distribution);
            m_DistrSum += _Distribution;
        }
    }
}
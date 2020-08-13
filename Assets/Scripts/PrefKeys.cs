
using UnityEngine;

namespace Clickers
{
    public class Pref
    {
        private string m_Key;

        public string Value
        {
            get => PlayerPrefs.GetString(m_Key);
            set => PlayerPrefs.SetString(m_Key, value);
        }

        public Pref(string key)
        {
            m_Key = key;
        }
    }
}


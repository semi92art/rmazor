using System;
using Common.Enums;
using Common.Helpers;
using UnityEngine;

namespace RMAZOR.Helpers
{
    [Serializable]
    public class LanguageInfo
    {
        public ELanguage language;
        public Sprite    flag;
    }
    
    [Serializable]
    public class LanguageInfosSet : ReorderableArray<LanguageInfosSet> { }
    
    [CreateAssetMenu(fileName = "lang_info", menuName = "Configs and Sets/Language Flags", order = 0)]
    public class LanguageInfoScriptableObject : ScriptableObject
    {
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using Common.Extensions;
using Common.Settings;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Common.Managers
{
    public interface ILocalizationManager : IInit
    {
        string   GetTranslation(string _Key);
        void     SetLanguage(Language  _Language);
        Language GetCurrentLanguage();
        void     AddTextObject(Component _TextObject, string _LocalizationKey, Func<string, string> _TextFormula = null);
    }

    public class LeanLocalizationManager : ILocalizationManager
    {
        #region types

        private class TextObjectArgs
        {
            public Component            TextObject { get; set; }
            public Func<string, string> Formula    { get; set; }
        }

        #endregion
    
        #region nonpublic members

        private LeanLocalization m_Localization;
        private readonly Dictionary<string, List<TextObjectArgs>> m_TextObjectsDict =
            new Dictionary<string, List<TextObjectArgs>>(); 

        #endregion

        #region inject

        private ILanguageSetting LanguageSetting { get; }
    
        public LeanLocalizationManager(ILanguageSetting _LanguageSetting)
        {
            LanguageSetting = _LanguageSetting;
        }

        #endregion

        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;

        public void Init()
        {
            LanguageSetting.OnValueSet = SetLanguage;
            LanguageSetting.GetValue = GetCurrentLanguage;
            if (m_Localization != null)
                return;
            var go = new GameObject("Localization");
            Object.DontDestroyOnLoad(go);
            m_Localization = go.AddComponent<LeanLocalization>();
            var culturesDict = new Dictionary<Language, string[]>
            {
                {Language.English,  new[] {"en", "en-GB"}},
                {Language.German,   new[] {"ger", "ger-GER"}},
                {Language.Spanish,  new[] {"sp", "sp-SP"}},
                {Language.Portugal, new[] {"por", "por-POR"}},
                {Language.Russian,  new[] {"ru", "ru-RUS"}}
            };
            foreach (var kvp in culturesDict)
                m_Localization.AddLanguage(kvp.Key.ToString(), kvp.Value);
            foreach (var kvp in culturesDict)
            {
                var goCsv = new GameObject(kvp.Key + "CSV");
                goCsv.SetParent(m_Localization.transform);
                var leanCsv = goCsv.AddComponent<LeanLanguageCSV>();
                leanCsv.Source = Resources.Load<TextAsset>($"Texts/{kvp.Key}");
                leanCsv.Language = kvp.Key.ToString();    
            }
            m_Localization.DefaultLanguage = "English";
            Initialize?.Invoke();
            Initialized = true;
        }

        public string GetTranslation(string _Key)
        {
            return LeanLocalization.GetTranslationText(_Key);
        }

        public void SetLanguage(Language _Language)
        {
            m_Localization.SetCurrentLanguage(_Language.ToString());
            foreach (var kvp in m_TextObjectsDict)
            {
                var destroyed = kvp.Value.ToArray()
                    .Where(_Args => _Args == null || _Args.TextObject.IsNull());
                kvp.Value.RemoveRange(destroyed);
            }
            foreach (var kvp in m_TextObjectsDict)
            {
                foreach (var args in kvp.Value)
                    UpdateText(kvp.Key, args);
            }
        }

        public Language GetCurrentLanguage()
        {
            Enum.TryParse(LeanLocalization.CurrentLanguage, out Language lang);
            return lang;
        }

        public void AddTextObject(Component _TextObject, string _LocalizationKey, Func<string, string> _TextFormula = null)
        {
            if (_TextObject.IsNull())
                return;
            foreach (var kvp in m_TextObjectsDict)
            {
                var destroyed = kvp.Value.Where(_Args => _Args == null || _Args.TextObject.IsNull());
                kvp.Value.RemoveRange(destroyed);
            }
            if (!m_TextObjectsDict.ContainsKey(_LocalizationKey))
                m_TextObjectsDict.Add(_LocalizationKey, new List<TextObjectArgs>());
            var args = m_TextObjectsDict[_LocalizationKey].FirstOrDefault(_Args => _Args.TextObject == _TextObject);
            if (args != null)
            {
                args.Formula = _TextFormula;
                UpdateText(_LocalizationKey, args);
            }
            else
            {
                var newArgs = new TextObjectArgs {TextObject = _TextObject, Formula = _TextFormula};
                m_TextObjectsDict[_LocalizationKey].Add(newArgs);
                UpdateText(_LocalizationKey, newArgs);
            }
        }

        #endregion

        #region nonpublic methods

        private void UpdateText(string _LocalizationKey, TextObjectArgs _Args)
        {
            if (_Args.TextObject is TMP_Text tmpText)
            {
                var translation = GetTranslation(_LocalizationKey);
                var formula = _Args.Formula ?? (_Text => _Text);
                tmpText.text = formula(translation);
            }
        }

        #endregion
    }
}
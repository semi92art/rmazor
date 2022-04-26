using System;
using System.Collections.Generic;
using System.Linq;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Settings;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Common.Managers
{
    public class LocalizableTextObjectInfo : ICloneable
    {
        public Component            TextObject      { get; }
        public string               LocalizationKey { get; set; }
        public ETextType            TextType        { get; set; }
        public Func<string, string> TextFormula     { get; set; }
        public bool                 AutoFont        { get; }

        public LocalizableTextObjectInfo(
            Component            _TextObject,
            ETextType            _TextType,
            string               _LocalizationKey = null,
            Func<string, string> _TextFormula = null,
            bool                 _AutoFont = true)
        {
            TextObject      = _TextObject;
            TextType        = _TextType;
            LocalizationKey = _LocalizationKey;
            TextFormula     = _TextFormula;
            AutoFont        = _AutoFont;
        }

        public object Clone()
        {
            return new LocalizableTextObjectInfo(TextObject, TextType, LocalizationKey, TextFormula);
        }
    }
    
    public interface ILocalizationManager : IInit, IFontProvider
    {
        event UnityAction<ELanguage> LanguageChanged;
        string                       GetTranslation(string _Key);
        void                         SetLanguage(ELanguage _Language);
        ELanguage                    GetCurrentLanguage();
        void                         AddTextObject(LocalizableTextObjectInfo _Info);
    }

    public class LeanLocalizationManager : InitBase, ILocalizationManager
    {
        #region constants

        private const string KeyEmpty = "empty_key";

        #endregion
    
        #region nonpublic members

        private LeanLocalization m_Localization;
        private readonly Dictionary<string, List<LocalizableTextObjectInfo>> m_TextObjectsDict =
            new Dictionary<string, List<LocalizableTextObjectInfo>>(); 

        #endregion

        #region inject

        private ILanguageSetting LanguageSetting { get; }
        private IFontProvider    FontProvider    { get; }

        public LeanLocalizationManager(
            ILanguageSetting _LanguageSetting,
            IFontProvider    _FontProvider)
        {
            LanguageSetting = _LanguageSetting;
            FontProvider    = _FontProvider;
        }

        #endregion

        #region api
        
        public event UnityAction<ELanguage> LanguageChanged;
        
        public override void Init()
        {
            if (Initialized)
                return;
            LanguageSetting.ValueSet += SetLanguage;
            LanguageSetting.GetValue = GetCurrentLanguage;
            if (m_Localization != null)
                return;
            var go = new GameObject("Localization");
            Object.DontDestroyOnLoad(go);
            m_Localization = go.AddComponent<LeanLocalization>();
            var culturesDict = new Dictionary<ELanguage, string[]>
            {
                {ELanguage.English,   new[] {"en",  "en-GB"}},
                {ELanguage.German,    new[] {"ger", "ger-GER"}},
                {ELanguage.Spanish,   new[] {"sp",  "sp-SP"}},
                {ELanguage.Portugal,  new[] {"por", "por-POR"}},
                {ELanguage.Russian,   new[] {"ru",  "ru-RUS"}},
                {ELanguage.Japaneese, new[] {"ja",  "ja-JAP"}},
                {ELanguage.Korean,    new[] {"ko",  "ko-KOR"}},
            };
            foreach (var (key, value) in culturesDict)
                m_Localization.AddLanguage(key.ToString(), value);
            foreach (var (key, _) in culturesDict)
            {
                var goCsv = new GameObject(key + "CSV");
                goCsv.SetParent(m_Localization.transform);
                var leanCsv = goCsv.AddComponent<LeanLanguageCSV>();
                leanCsv.Source = Resources.Load<TextAsset>($"Texts/{key}");
                leanCsv.Language = key.ToString();    
            }
            m_Localization.DefaultLanguage = "English";
            LeanLocalization.OnLocalizationChanged += () => LanguageChanged?.Invoke(GetCurrentLanguage());
            base.Init();
        }
        
        public string GetTranslation(string _Key)
        {
            return LeanLocalization.GetTranslationText(_Key);
        }

        public void SetLanguage(ELanguage _Language)
        {
            m_Localization.SetCurrentLanguage(_Language.ToString());
            foreach (var (_, value) in m_TextObjectsDict)
            {
                var destroyed = value.ToArray()
                    .Where(_Args => _Args == null || _Args.TextObject.IsNull());
                value.RemoveRange(destroyed);
            }
            foreach (var (_, value) in m_TextObjectsDict)
            {
                foreach (var args in value)
                    UpdateText(args);
            }
        }

        public ELanguage GetCurrentLanguage()
        {
            Enum.TryParse(LeanLocalization.CurrentLanguage, out ELanguage lang);
            return lang;
        }

        public void AddTextObject(LocalizableTextObjectInfo _Info)
        {
            if (_Info.TextObject.IsNull())
                return;
            if (string.IsNullOrEmpty(_Info.LocalizationKey))
                _Info.LocalizationKey = KeyEmpty;
            foreach (var (_, value) in m_TextObjectsDict)
            {
                var destroyed = value
                    .Where(_Args => _Args == null || _Args.TextObject.IsNull());
                value.RemoveRange(destroyed);
            }
            if (!m_TextObjectsDict.ContainsKey(_Info.LocalizationKey))
                m_TextObjectsDict.Add(_Info.LocalizationKey, new List<LocalizableTextObjectInfo>());
            var args = m_TextObjectsDict[_Info.LocalizationKey]
                .FirstOrDefault(_Args => _Args.TextObject == _Info.TextObject);
            if (args != null)
            {
                args.TextFormula = _Info.TextFormula;
                args.TextType = _Info.TextType;
                UpdateText(args);
            }
            else
            {
                var newArgs = _Info.Clone() as LocalizableTextObjectInfo;
                m_TextObjectsDict[_Info.LocalizationKey].Add(newArgs);
                UpdateText(newArgs);
            }
        }
        
        public TMP_FontAsset GetFont(ETextType _TextType, ELanguage _Language)
        {
            return FontProvider.GetFont(_TextType, _Language);
        }

        #endregion

        #region nonpublic methods

        private void UpdateText(LocalizableTextObjectInfo _Info)
        {
            if (!(_Info.TextObject is TMP_Text tmpText))
                return;
            if (_Info.LocalizationKey != KeyEmpty)
            {
                string translation = GetTranslation(_Info.LocalizationKey);
                string Formula(string _Text) => _Info.TextFormula == null ? _Text : _Info.TextFormula(_Text);
                tmpText.text = Formula(translation);
            }
            if (_Info.AutoFont)
                tmpText.font = GetFont(_Info.TextType, GetCurrentLanguage());
        }

        #endregion
    }
}
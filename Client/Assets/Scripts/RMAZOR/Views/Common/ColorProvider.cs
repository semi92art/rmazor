using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Settings;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace RMAZOR.Views.Common
{
    public class ColorProvider : MonoBehInitBase, IColorProvider
    {
        #region nonpublic members

        private readonly Dictionary<int, Color>   m_ColorsDict = new Dictionary<int, Color>();
        private          IList<MainColorsSetItem> m_CurrentSet;
        private          IList<MainColorsSetItem> m_LightThemeSet;
        private          IList<MainColorsSetItem> m_DarkThemeSet;
        private readonly List<int>                m_IgnorableForThemeSwitchColorIds = new List<int>();

        #endregion

        #region inject
        
        private RemoteProperties  RemoteProperties { get; set; }
        private IPrefabSetManager PrefabSetManager { get; set; }
        private IDarkThemeSetting DarkThemeSetting { get; set; }

        [Inject]
        public void Inject(
            RemoteProperties  _RemoteProperties,
            IPrefabSetManager _PrefabSetManager,
            IDarkThemeSetting _DarkThemeSetting)
        {
            RemoteProperties = _RemoteProperties;
            PrefabSetManager = _PrefabSetManager;
            DarkThemeSetting = _DarkThemeSetting;
        }

        #endregion

        #region api
        
        public event UnityAction<int, Color>  ColorChanged;
        public event UnityAction<EColorTheme> ColorThemeChanged;

        public override void Init()
        {
            m_LightThemeSet = RemoteProperties.MainColorsSet;
            if (m_LightThemeSet.NullOrEmpty())
            {
                m_LightThemeSet = PrefabSetManager.GetObject<MainColorsSetScriptableObject>(
                    "views", "color_set_light").set;
            }
            m_DarkThemeSet = PrefabSetManager.GetObject<MainColorsSetScriptableObject>(
                "views", "color_set_dark").set;
            SetThemeCore(DarkThemeSetting.Get());
            DarkThemeSetting.OnValueSet = _Value => SetTheme(_Value ? EColorTheme.Dark : EColorTheme.Light);
            base.Init();
        }

        public bool DarkThemeAvailable
        {
            get => SaveUtils.GetValue(SaveKeysRmazor.DarkThemeAvailable);
            set => SaveUtils.PutValue(SaveKeysRmazor.DarkThemeAvailable, value);
        }

        public EColorTheme CurrentTheme => DarkThemeSetting.Get() ? EColorTheme.Dark : EColorTheme.Light;
        
        public void AddIgnorableForThemeSwitchColor(int    _ColorId)
        {
            m_IgnorableForThemeSwitchColorIds.Add(_ColorId);
        }

        public void RemoveIgnorableForThemeSwitchColor(int _ColorId)
        {
            m_IgnorableForThemeSwitchColorIds.Remove(_ColorId);
        }

        public Color GetColor(int _Id)
        {
            if (!Initialized)
                return default;
            if (m_ColorsDict.ContainsKey(_Id)) 
                return m_ColorsDict[_Id];
            Dbg.LogWarning($"Color \"{ColorIds.GetColorNameById(_Id)}\" with key \"{_Id}\" was not set.");
            return default;
        }

        public void SetColor(int _Id, Color _Color)
        {
            if (!Initialized)
                return;
            m_ColorsDict.SetSafe(_Id, _Color);
            ColorChanged?.Invoke(_Id, _Color);
        }
        
        public virtual void SetTheme(EColorTheme _Theme)
        {
            SaveUtils.PutValue(SaveKeysCommon.DarkTheme, _Theme == EColorTheme.Dark);
            SetThemeCore(_Theme == EColorTheme.Dark);
            foreach (int id in m_ColorsDict.Keys.ToList().Except(m_IgnorableForThemeSwitchColorIds))
                SetColor(id, m_ColorsDict[id]);
            ColorThemeChanged?.Invoke(_Theme);
        }
        
        #endregion

        #region nonpublic methods
        
        private void SetThemeCore(bool _Dark)
        {
            m_CurrentSet = _Dark ? m_DarkThemeSet : m_LightThemeSet;
            if (_Dark && !DarkThemeAvailable)
            {
                Dbg.LogError("Try to set dark theme while it is not available");
                m_CurrentSet = m_LightThemeSet;
            }
            m_ColorsDict.Clear();
            foreach (var item in m_CurrentSet)
                m_ColorsDict.Add(ColorIdsCommon.GetHash(item.name), item.color);
        }

        #endregion
    }
}
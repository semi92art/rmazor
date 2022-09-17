using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Settings;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace RMAZOR.Views.Common
{
    public class ColorProvider : MonoBehInitBase, IColorProvider
    {
        #region nonpublic members

        private readonly Dictionary<int, Color> m_ColorsDict = new Dictionary<int, Color>();
        private readonly List<int>              m_IgnorableForThemeSwitchColorIds = new List<int>();
        
        private          IList<MainColorsProps> m_Set;

        #endregion

        #region inject
        
        private IRemotePropertiesRmazor RemoteProperties { get; set; }
        private IPrefabSetManager       PrefabSetManager { get; set; }

        [Inject]
        private void Inject(
            IRemotePropertiesRmazor _RemoteProperties,
            IPrefabSetManager       _PrefabSetManager)
        {
            RemoteProperties = _RemoteProperties;
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api
        
        public event UnityAction<int, Color>  ColorChanged;

        public override void Init()
        {
            m_Set = RemoteProperties.MainColorsSet;
            if (m_Set.NullOrEmpty())
            {
                m_Set = PrefabSetManager.GetObject<MainColorsSetScriptableObject>(
                    "views", "color_set_light").set;
            }
            m_ColorsDict.Clear();
            foreach (var item in m_Set)
            {
                try
                {
                    m_ColorsDict.Add(ColorIds.GetColorIdByName(item.name), item.color);
                }
                catch (System.ArgumentException ex)
                {
                    Dbg.LogError(ex);
                }
            }
            foreach (int id in m_ColorsDict.Keys.Except(m_IgnorableForThemeSwitchColorIds))
                SetColor(id, m_ColorsDict[id]);
            base.Init();
        }

        public void AddIgnorableForThemeSwitchColor(int _ColorId)
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

        #endregion
    }
}
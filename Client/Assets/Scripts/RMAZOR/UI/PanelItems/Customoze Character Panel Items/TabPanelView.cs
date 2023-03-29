using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items
{
    public class Badge
    {
        [JsonProperty("N")] public int Number { get; set; }
    }
    
    public class TabPanelItemArgs
    {
        public string      LocalizationKey { get; set; }
        public UnityAction OnClick         { get; set; }
    }

    [Serializable]
    public class TabPanelItem
    {
        public Button          button;
        public TextMeshProUGUI title;
        public Image           badge;
        public TextMeshProUGUI badgeText;
    }
    
    public class TabPanelView : SimpleUiItem
    {
        #region serialized fields

        [SerializeField] private RectTransform  selector;
        [SerializeField] private TabPanelItem[] tabItems;

        #endregion

        #region nonpublic members

        private string m_TabId;
        
        private Dictionary<int, TabPanelItemArgs> m_TabPanelPropsDict 
            = new Dictionary<int, TabPanelItemArgs>();
        
        #endregion

        #region api

        public event UnityAction<List<Badge>> BadgesChanged;

        public List<Badge> GetBadges()
        {
            var badgeListsDict = SaveUtils.GetValue(SaveKeysRmazor.TabBadgesDict);
            if (badgeListsDict == null)
            {
                badgeListsDict = new Dictionary<string, List<Badge>>();
                SaveUtils.PutValue(SaveKeysRmazor.TabBadgesDict, badgeListsDict);
            }
            if (badgeListsDict.ContainsKey(m_TabId)) 
                return badgeListsDict[m_TabId];
            var defaultBadgesList = tabItems.Select(_I => new Badge {Number = 1}).ToList();
            badgeListsDict.Add(m_TabId,defaultBadgesList);
            SaveUtils.PutValue(SaveKeysRmazor.TabBadgesDict, badgeListsDict);
            return badgeListsDict[m_TabId];
        }

        public void Init(
            IUITicker                          _UITicker,
            IAudioManager                      _AudioManager,
            ILocalizationManager               _LocalizationManager,
            Dictionary<int, TabPanelItemArgs> _TabPanelProps,
            string _TabId)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager);
            m_TabPanelPropsDict = _TabPanelProps;
            m_TabId             = _TabId;
            LocalizeTextObjectsOnInit();
            SelectTabItem(0);
        }

        #endregion

        #region nonpublic members

        private void UpdateState()
        {
            var badges = GetBadges();
            int k = 0;
            foreach (var badge in badges)
            {
                tabItems[k]  .badgeText.text    = badge.Number.ToString();
                tabItems[k]  .badge    .enabled = badge.Number > 0;
                tabItems[k++].badgeText.enabled = badge.Number > 0;
            }
        }

        private void LocalizeTextObjectsOnInit()
        {
            foreach ((int key, var value) in m_TabPanelPropsDict)
            {
                var tabItem = tabItems[key];
                var locTextInfo = new LocTextInfo(tabItems[key].title, ETextType.MenuUI_H1, value.LocalizationKey,
                    _T => _T.ToUpper(CultureInfo.InvariantCulture));
                LocalizationManager.AddLocalization(locTextInfo);
                tabItem.button.SetOnClick(() =>
                {
                    SelectTabItem(key);
                    value.OnClick?.Invoke();
                });
            }
        }

        private void SelectTabItem(int _Idx)
        {
            SelectTabItemCore(_Idx);
            UpdateState();
        }

        private void SelectTabItemCore(int _Idx)
        {
            var tabItem = tabItems[_Idx];
            selector.SetParent(tabItem.button.transform);
            selector.SetParams(
                UiAnchor.Create(0.5f, 0f, 0.5f, 0f),
                new Vector2(0f, -2f),
                Vector2.one * 0.5f,
                new Vector2(146f, 12f));
            foreach (var title in tabItems.Select(_I => _I.title))
                title.color = new Color(0.29f, 0.67f, 0.97f);
            tabItem.title.color = Color.white;
            var badgesEnabled = GetBadges();
            badgesEnabled[_Idx].Number = 0;
            var badgesDict = SaveUtils.GetValue(SaveKeysRmazor.TabBadgesDict);
            badgesDict[m_TabId] = badgesEnabled;
            SaveUtils.PutValue(SaveKeysRmazor.TabBadgesDict, badgesDict);
            BadgesChanged?.Invoke(GetBadges());
        }

        #endregion
    }
}
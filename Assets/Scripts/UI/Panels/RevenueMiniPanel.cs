using System;
using System.Collections;
using System.Collections.Generic;
using Constants;
using Entities;
using Extensions;
using GameHelpers;
using Managers;
using TMPro;
using UI.Entities;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public interface IRevenueMiniPanel : IMiniPanel
    {
        void PlusRevenue(BankItemType _BankItemType, long _Revenue);
        void ClearRevenue();
    }
    
    public enum RevenuePanelPosition
    {
        Top,
        TopLeft,
        TopRight
    }
    
    public class RevenueMiniPanel : IRevenueMiniPanel
    {

        #region nonpublic members
        
        private readonly RectTransform m_Panel;
        private readonly TextMeshProUGUI m_BankMoney;
        private readonly TextMeshProUGUI m_RevenueText;
        private readonly Image m_RevenueIcon;
        private readonly Dictionary<string, bool> m_Coroutines = new Dictionary<string, bool>();
        private readonly Dictionary<BankItemType, long> m_Revenue = new Dictionary<BankItemType, long>
        {
            {BankItemType.Gold, 0},
            {BankItemType.Diamonds, 0},
            {BankItemType.Lifes, 0}
        };

        private BankItemType m_LastRevenueType;
        private long m_LastRevenue;
        private bool m_RevenueShowing;
        private string m_LastCoroutineHash;
        
        #endregion
        
        #region constructor

        private RevenueMiniPanel(RectTransform _Parent, RevenuePanelPosition _Position)
        {
            string prefabName = "revenue_mini_panel_top";
            if (_Position == RevenuePanelPosition.TopLeft)
                prefabName = "revenue_mini_panel_top_left";
            else if (_Position == RevenuePanelPosition.TopRight)
                prefabName = "revenue_mini_panel_top_right";
            
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    UiAnchor.Create(1, 1, 1, 1),
                    new Vector2(0, -65f), 
                    Vector2.one,
                    new Vector2(390f, 83f)),
                "game_menu", prefabName);
            m_Panel = go.RTransform();
            m_BankMoney = go.GetCompItem<TextMeshProUGUI>("bank_money");
            m_RevenueText = go.GetCompItem<TextMeshProUGUI>("revenue");
            m_RevenueIcon = go.GetCompItem<Image>("revenue_icon");
        }
        
        #endregion
        
        #region api

        public static IRevenueMiniPanel Create(RectTransform _Parent, RevenuePanelPosition _Position)
        {
            return new RevenueMiniPanel(_Parent, _Position);
        }
        
        public void Show()
        {
            m_RevenueShowing = true;
            m_Panel.SetGoActive(true);
        }

        public void Hide()
        {
            m_Panel.gameObject.SetActive(false);
        }

        public void Init()
        {
            
        }

        public void PlusRevenue(BankItemType _BankItemType, long _Revenue)
        {
            CheckForLastRevenueFinished();
            m_LastRevenueType = _BankItemType;
            m_LastRevenue = _Revenue;

            string newHash = Md5.GetMd5String(BitConverter.GetBytes(CommonUtils.RandomGen.NextDouble()));
            m_Coroutines.Add(newHash, true);

            if (!string.IsNullOrEmpty(m_LastCoroutineHash))
                m_Coroutines[m_LastCoroutineHash] = false;
            m_LastCoroutineHash = newHash;
            
            m_BankMoney.text = m_Revenue[_BankItemType].ToNumeric();
            m_RevenueText.text = "+ " + _Revenue.ToNumeric();
            m_RevenueIcon.sprite = PrefabInitializer.GetObject<Sprite>("coins",
                _BankItemType == BankItemType.Gold ? "gold_coin_0" : "diamond_coin_0");
            Show();
            m_RevenueShowing = true;
            Coroutines.Run(Coroutines.Delay(() =>
            {
                m_BankMoney.text = (m_Revenue[_BankItemType] + _Revenue).ToNumeric();
                Coroutines.Run(Coroutines.Delay(
                    FinishAnimate, 0.5f, 
                    () => !m_Coroutines[newHash]));
            },
            0.2f, () => !m_Coroutines[newHash]));
        }

        public void ClearRevenue()
        {
            foreach (var mt in CommonUtils.EnumToList<BankItemType>())
                m_Revenue[mt] = 0;
        }
        
        #endregion
        
        #region nonpublic methods

        private void FinishAnimate()
        {
            m_RevenueShowing = false;
            RevenueToTempBank();
            Hide();
        }

        private void RevenueToTempBank()
        {
            m_Revenue[m_LastRevenueType] += m_LastRevenue;
        }

        private void CheckForLastRevenueFinished()
        {
            if (!m_RevenueShowing) 
                return;
            RevenueToTempBank();
        }
        
        #endregion
    }
}
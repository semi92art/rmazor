using System;
using System.Collections;
using System.Collections.Generic;
using Constants;
using Extensions;
using Helpers;
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
        void PlusRevenue(MoneyType _MoneyType, long _Revenue);
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
        private readonly TextMeshProUGUI m_Revenue;
        private readonly Image m_RevenueIcon;
        private readonly Dictionary<string, bool> m_Coroutines = new Dictionary<string, bool>();

        private MoneyType m_LastRevenueType;
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
            m_Revenue = go.GetCompItem<TextMeshProUGUI>("revenue");
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
            m_Panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            m_Panel.gameObject.SetActive(false);
        }

        public void PlusRevenue(MoneyType _MoneyType, long _Revenue)
        {
            CheckForLastRevenueFinished();
            m_LastRevenueType = _MoneyType;
            m_LastRevenue = _Revenue;
            var bank = MoneyManager.Instance.GetBank();
            
            string newHash = Md5.GetMd5String(BitConverter.GetBytes(Utility.RandomGen.NextDouble()));
            m_Coroutines.Add(newHash, true);

            if (!string.IsNullOrEmpty(m_LastCoroutineHash))
                m_Coroutines[m_LastCoroutineHash] = false;
            m_LastCoroutineHash = newHash;
            
            Coroutines.Run(Coroutines.WaitWhile(() =>
            {
                m_BankMoney.text = bank.Money[_MoneyType].ToNumeric();
                m_Revenue.text = "+ " + _Revenue.ToNumeric();
                m_RevenueIcon.sprite = PrefabInitializer.GetObject<Sprite>("coins",
                    _MoneyType == MoneyType.Gold ? "gold_coin_0" : "diamond_coin_0");
                Show();
                m_RevenueShowing = true;
                Coroutines.Run(Coroutines.Delay(() =>
                {
                    m_BankMoney.text = (bank.Money[_MoneyType] + _Revenue).ToNumeric();
                    Coroutines.Run(Coroutines.Delay(
                        FinishAnimate, 0.5f, 
                        () => !m_Coroutines[newHash]));
                },
                0.2f, () => !m_Coroutines[newHash]));
            }, () => !bank.Loaded, () => !m_Coroutines[newHash]));
        }
        
        #endregion
        
        #region nonpublic methods

        private void FinishAnimate()
        {
            m_RevenueShowing = false;
            RevenueToBank();
            Hide();
        }

        private void RevenueToBank()
        {
            MoneyManager.Instance.PlusMoney(new Dictionary<MoneyType, long>{{m_LastRevenueType, m_LastRevenue}});
        }

        private void CheckForLastRevenueFinished()
        {
            if (!m_RevenueShowing) 
                return;
            RevenueToBank();
        }
        
        #endregion
    }
}
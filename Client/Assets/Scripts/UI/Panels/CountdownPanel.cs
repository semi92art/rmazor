using Extensions;
using GameHelpers;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityGameLoopDI;

namespace UI.Panels
{
    public class CountdownPanel : DialogPanelBase, IGameUiCategory
    {
        #region nonpublic members

        private readonly RectTransform m_Container;
        private readonly UnityAction m_OnCountdownFinish;

        #endregion

        #region api

        public GameUiCategory Category => GameUiCategory.Countdown;

        public CountdownPanel(RectTransform _Container, UnityAction _OnCountdownFinish, ITicker _Ticker) : base(_Ticker)
        {
            m_Container = _Container;
            m_OnCountdownFinish = _OnCountdownFinish;
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Container,
                    RtrLites.FullFill),
                "game_menu", "countdown_panel");
            var view = go.GetCompItem<CountdownPanelView>("view");
            view.StartCountdown(() => m_OnCountdownFinish?.Invoke());
            Panel = go.RTransform();
            go.SetActive(false);
        }

        public override void Init()
        {
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Container,
                    RtrLites.FullFill),
                "game_menu", "countdown_panel");
            var view = go.GetCompItem<CountdownPanelView>("view");
            view.StartCountdown(() => m_OnCountdownFinish?.Invoke());
            Panel = go.RTransform();
        }

        #endregion

    }
}
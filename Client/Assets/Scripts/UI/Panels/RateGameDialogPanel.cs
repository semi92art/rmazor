using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;
using TMPro;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public interface IRateGameDialogPanel : IDialogPanel { }
    
    public class RateGameDialogPanel : DialogPanelBase, IRateGameDialogPanel
    {
        #region nonpublic members

        private Animator           m_Animator;
        private AnimationTriggerer m_Triggerer;
        private Button             m_ButtonClose;
        private Button             m_ButtonRateGame;
        private TextMeshProUGUI    m_TextRateGame;
        private Image              m_Stars;

        #endregion

        #region inject
        
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }
        private ViewSettings                ViewSettings         { get; }

        public RateGameDialogPanel(
            IManagersGetter _Managers,
            IUITicker _Ticker,
            IBigDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IProposalDialogViewer _ProposalDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder,
            ViewSettings _ViewSettings)
            : base( _Managers, _Ticker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            ProposalDialogViewer = _ProposalDialogViewer;
            CommandsProceeder = _CommandsProceeder;
            ViewSettings = _ViewSettings;
        }

        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.RateGame;
        public override bool        AllowMultiple => false;

        public override void LoadPanel()
        {
            base.LoadPanel();
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    ProposalDialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "rate_game_panel");
            PanelObject = go.RTransform();
            go.SetActive(false);
            m_Animator        = go.GetCompItem<Animator>("animator");
            m_Triggerer       = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonClose     = go.GetCompItem<Button>("close_button");
            m_ButtonRateGame  = go.GetCompItem<Button>("rate_game_button");
            m_TextRateGame    = go.GetCompItem<TextMeshProUGUI>("rate_game_text");
            m_Stars           = go.GetCompItem<Image>("stars");
            m_Triggerer.Trigger1 = () => Coroutines.Run(OnPanelStartAnimationFinished());
            var panel = go.GetCompItem<SimpleUiDialogPanelView>("panel");
            panel.Init(Managers, Ticker, ColorProvider);
            var button = go.GetCompItem<SimpleUiButtonView>("rate_game_button");
            button.Init(Managers, Ticker, ColorProvider);
            button.Highlighted = true;
            m_ButtonClose.onClick.AddListener(OnCloseButtonClick);
            m_ButtonRateGame.onClick.AddListener(OnRateGameButtonClick);
            m_TextRateGame.text = Managers.LocalizationManager.GetTranslation("rate_game");
            m_Stars.color = ColorProvider.GetColor(ColorIds.UI);
            var closeButtonAnimator = m_ButtonClose.GetComponent<Animator>();
            if (closeButtonAnimator.IsNotNull())
                closeButtonAnimator.enabled = false;
        }
        
        public override void OnDialogShow()
        {
            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                CommandsProceeder.LockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
            }));
            m_Animator.speed = ViewSettings.ProposalDialogAnimSpeed;
            m_Animator.SetTrigger(AnimKeys.Anim);
            m_ButtonClose.SetGoActive(false);
        }

        public override void OnDialogHide()
        {
            CommandsProceeder.UnlockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
        }
        
        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UI)
            {
                m_Stars.color = _Color;
            }
        }

        private IEnumerator OnPanelStartAnimationFinished()
        {
            const float closeButtonEnableDelay = 3f;
            float time = Ticker.Time;
            yield return Coroutines.WaitWhile(
                () => time + closeButtonEnableDelay > Ticker.Time,
                () => m_ButtonClose.SetGoActive(true));
        }
        
        private void OnRateGameButtonClick()
        {
            Managers.ShopManager.RateGame(false);
            SaveUtils.PutValue(SaveKeys.GameWasRated, true);
            ProposalDialogViewer.Back();
            
        }

        private void OnCloseButtonClick()
        {
            ProposalDialogViewer.Back();
        }

        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return RazorMazeUtils.GetMoveCommands()
                .Concat(RazorMazeUtils.GetRotateCommands())
                .Concat(new[]
                {
                    EInputCommand.ReadyToUnloadLevel,
                    EInputCommand.ShopMenu,
                    EInputCommand.SettingsMenu
                });
        }

        #endregion
    }
}
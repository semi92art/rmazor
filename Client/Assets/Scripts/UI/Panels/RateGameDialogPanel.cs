using System.Collections;
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
        private Image              m_IconCharacter;
        private Image              m_Eye1;
        private Image              m_Eye2;

        #endregion

        #region inject
        
        private IProposalDialogViewer ProposalDialogViewer { get; }
        private IViewInput            Input                { get; }
        private ViewSettings          ViewSettings         { get; }

        public RateGameDialogPanel(
            IManagersGetter _Managers,
            IUITicker _Ticker,
            IBigDialogViewer _DialogViewer,
            ICameraProvider _CameraProvider,
            IColorProvider _ColorProvider,
            IProposalDialogViewer _ProposalDialogViewer,
            IViewInput _Input,
            ViewSettings _ViewSettings)
            : base(_Managers, _Ticker, _DialogViewer, _CameraProvider, _ColorProvider)
        {
            ProposalDialogViewer = _ProposalDialogViewer;
            Input = _Input;
            ViewSettings = _ViewSettings;
        }

        #endregion

        #region api

        public override EUiCategory Category => EUiCategory.RateGame;
        
        public override void Init()
        {
            base.Init();
            var go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    ProposalDialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.DialogPanels, "rate_game_panel");
            Panel = go.RTransform();
            go.SetActive(false);

            m_Animator        = go.GetCompItem<Animator>("animator");
            m_Triggerer       = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonClose     = go.GetCompItem<Button>("close_button");
            m_ButtonRateGame  = go.GetCompItem<Button>("rate_game_button");
            m_TextRateGame    = go.GetCompItem<TextMeshProUGUI>("rate_game_text");
            m_IconCharacter   = go.GetCompItem<Image>("character_icon");
            m_Eye1            = go.GetCompItem<Image>("character_eye_1");
            m_Eye2            = go.GetCompItem<Image>("character_eye_2");

            m_Triggerer.Trigger1 = () => Coroutines.Run(OnPanelStartAnimationFinished());
            m_ButtonClose.onClick.AddListener(OnCloseButtonClick);
            m_ButtonRateGame.onClick.AddListener(OnRateGameButtonClick);
            m_TextRateGame.text = Managers.LocalizationManager.GetTranslation("rate_game");
            m_IconCharacter.color = ColorProvider.GetColor(ColorIds.Character);
            m_Eye1.color = m_Eye2.color = ColorProvider.GetColor(ColorIds.Background);
        }
        
        public override void OnDialogShow()
        {
            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                Input.LockCommand(InputCommands.ReadyToUnloadLevel);
                Input.LockCommand(InputCommands.ShopMenu);
                Input.LockCommand(InputCommands.SettingsMenu);
            }));
            m_Animator.speed = ViewSettings.ProposalDialogAnimSpeed;
            m_Animator.SetTrigger(AnimKeys.Anim);
            m_ButtonClose.SetGoActive(false);
        }

        public override void OnDialogHide()
        {
            Input.UnlockCommand(InputCommands.ReadyToUnloadLevel);
            Input.UnlockCommand(InputCommands.ShopMenu);
            Input.UnlockCommand(InputCommands.SettingsMenu);
        }

        #endregion

        #region nonpublic methods
        
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
            Managers.ShopManager.RateGame();
            SaveUtils.PutValue(SaveKey.GameWasRated, true);
            ProposalDialogViewer.Back();
            
        }

        private void OnCloseButtonClick()
        {
            ProposalDialogViewer.Back();
        }

        #endregion
    }
}
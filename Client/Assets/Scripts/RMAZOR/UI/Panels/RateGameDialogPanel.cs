using System.Collections;
using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IRateGameDialogPanel : IDialogPanel
    {
        bool CanBeClosed { get; set; }
        void SetDialogText(string _Text);
    }
    
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

        public RateGameDialogPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            IBigDialogViewer            _DialogViewer,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IProposalDialogViewer       _ProposalDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _Managers, 
                _Ticker, 
                _DialogViewer, 
                _CameraProvider,
                _ColorProvider)
        {
            ProposalDialogViewer = _ProposalDialogViewer;
            CommandsProceeder    = _CommandsProceeder;
        }

        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.RateGame;
        public override bool        AllowMultiple => false;

        public bool CanBeClosed
        {
            get => m_ButtonClose.gameObject.activeSelf;
            set => m_ButtonClose.SetGoActive(value);
        }

        public override void LoadPanel()
        {
            base.LoadPanel();
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    ProposalDialogViewer.Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "rate_game_panel");
            PanelObject = go.RTransform();
            go.SetActive(false);
            m_Animator        = go.GetCompItem<Animator>("animator");
            m_Triggerer       = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonClose     = go.GetCompItem<Button>("close_button");
            m_ButtonRateGame  = go.GetCompItem<Button>("rate_game_button");
            m_TextRateGame    = go.GetCompItem<TextMeshProUGUI>("rate_game_text");
            m_Stars           = go.GetCompItem<Image>("stars");
            m_Triggerer.Trigger1 = () => Cor.Run(OnPanelStartAnimationFinished());
            var panel = go.GetCompItem<SimpleUiDialogPanelView>("panel");
            panel.Init(Ticker, ColorProvider, Managers.AudioManager, Managers.LocalizationManager, Managers.PrefabSetManager);
            var button = go.GetCompItem<SimpleUiButtonView>("rate_game_button");
            button.Init(Ticker, ColorProvider, Managers.AudioManager, Managers.LocalizationManager, Managers.PrefabSetManager);
            button.Highlighted = true;
            m_ButtonClose.onClick.AddListener(OnCloseButtonClick);
            m_ButtonRateGame.onClick.AddListener(OnRateGameButtonClick);
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextRateGame, ETextType.MenuUI, "rate_game"));
            m_TextRateGame.text = Managers.LocalizationManager.GetTranslation("rate_game");
            m_Stars.color = ColorProvider.GetColor(ColorIds.UI);
            var closeButtonAnimator = m_ButtonClose.GetComponent<Animator>();
            if (closeButtonAnimator.IsNotNull())
                closeButtonAnimator.enabled = false;
        }
        
        public override void OnDialogShow()
        {
            Cor.Run(Cor.WaitEndOfFrame(() =>
            {
                CommandsProceeder.LockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
            }));
            m_Animator.speed = ProposalDialogViewer.AnimationSpeed;
            m_Animator.SetTrigger(AnimKeys.Anim);
            m_ButtonClose.SetGoActive(false);
        }

        public override void OnDialogHide()
        {
            CommandsProceeder.UnlockCommands(GetCommandsToLock(), nameof(IRateGameDialogPanel));
        }

        public void SetDialogText(string _Text)
        {
            m_TextRateGame.text = _Text;
        }

        

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UI)
                m_Stars.color = _Color;
        }

        private IEnumerator OnPanelStartAnimationFinished()
        {
            const float closeButtonEnableDelay = 3f;
            float time = Ticker.Time;
            yield return Cor.WaitWhile(
                () => time + closeButtonEnableDelay > Ticker.Time,
                () => m_ButtonClose.SetGoActive(true));
        }
        
        private void OnRateGameButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.RateGameButton2Pressed);
            Managers.ShopManager.RateGame(false);
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            ProposalDialogViewer.Back();
            
        }

        private void OnCloseButtonClick()
        {
            ProposalDialogViewer.Back();
        }

        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return RmazorUtils.MoveAndRotateCommands;
        }

        #endregion
    }

    public class RateGameDialogPanelFake : FakeDialogPanel, IRateGameDialogPanel
    {
        public bool CanBeClosed                 { get; set; }
        public void SetDialogText(string _Text) { }
    }
}
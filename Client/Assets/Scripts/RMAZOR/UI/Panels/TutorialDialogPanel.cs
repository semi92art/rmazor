using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities.UI;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace RMAZOR.UI.Panels
{
    public class TutorialDialogPanelInfo
    {
        public string TitleLocalizationKey       { get; }
        public string DescriptionLocalizationKey { get; }
        public string VideoClipAssetKey          { get; }
        
        public TutorialDialogPanelInfo(
            string _TitleLocalizationKey, 
            string _DescriptionLocalizationKey,
            string _VideoClipAssetKey)
        {
            TitleLocalizationKey       = _TitleLocalizationKey;
            DescriptionLocalizationKey = _DescriptionLocalizationKey;
            VideoClipAssetKey          = _VideoClipAssetKey;
        }
    }
    
    public interface ITutorialDialogPanel : IDialogPanel
    {
        bool IsVideoReady { get; }
        void SetPanelInfo(TutorialDialogPanelInfo _Info);
        void PrepareVideo();
    }
    
    public class TutorialDialogPanel : DialogPanelBase, ITutorialDialogPanel
    {
        #region nonpublic members

        private TutorialDialogPanelInfo m_Info;
        private TextMeshProUGUI         m_Title;
        private TextMeshProUGUI         m_Description;
        private VideoPlayer             m_VideoPlayer;
        private Button                  m_ButtonClose;
        private Animator                m_Animator;

        #endregion

        #region inject
        
        private IContainersGetter                   ContainersGetter               { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        public TutorialDialogPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker) 
            : base(
                _Managers, 
                _Ticker,
                _CameraProvider,
                _ColorProvider)
        {
            ContainersGetter     = _ContainersGetter;
            CommandsProceeder    = _CommandsProceeder;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion

        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override Animator          Animator         => m_Animator;

        public bool IsVideoReady => m_VideoPlayer.IsNotNull() && m_VideoPlayer.isPlaying;

        public void SetPanelInfo(TutorialDialogPanelInfo _Info)
        {
            m_Info = _Info;
        }

        public void PrepareVideo()
        {
            if (m_VideoPlayer.IsNull())
                InitVideoPlayer();
            m_VideoPlayer.clip = Managers.PrefabSetManager.GetObject<VideoClip>(
                "tutorial_clips", m_Info.VideoClipAssetKey);
            m_VideoPlayer.enabled = true;
            m_VideoPlayer.Play();
        }

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels,
                "tutorial_panel");
            PanelRectTransform = go.RTransform();
            var panel = go.GetCompItem<SimpleUiDialogPanelView>("panel");
            panel.Init(Ticker, Managers.AudioManager, Managers.LocalizationManager);
            m_Title = go.GetCompItem<TextMeshProUGUI>("title");
            m_Description = go.GetCompItem<TextMeshProUGUI>("description");
            m_ButtonClose = go.GetCompItem<Button>("button_close");
            m_Animator    = go.GetCompItem<Animator>("animator");
            m_ButtonClose.onClick.AddListener(OnCloseButtonClick);
        }

        public override void OnDialogStartAppearing()
        {
            var locInfoTitle = new LocalizableTextObjectInfo(
                m_Title, ETextType.MenuUI, m_Info.TitleLocalizationKey,
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            Managers.LocalizationManager.RemoveTextObject(locInfoTitle);
            Managers.LocalizationManager.AddTextObject(locInfoTitle);
            var locInfoDescription = new LocalizableTextObjectInfo(
                m_Description, ETextType.MenuUI, m_Info.DescriptionLocalizationKey,
                _T => _T.ToUpper(CultureInfo.CurrentUICulture));
            Managers.LocalizationManager.RemoveTextObject(locInfoDescription);
            Managers.LocalizationManager.AddTextObject(locInfoDescription);
            CommandsProceeder.LockCommands(
                GetCommandsToLock(), 
                nameof(ITutorialDialogPanel));
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            CommandsProceeder.UnlockCommands(
                GetCommandsToLock(), 
                nameof(ITutorialDialogPanel));
            m_VideoPlayer.Stop();
            m_VideoPlayer.clip = null;
            m_VideoPlayer.enabled = false;
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel, true);
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods

        private void OnCloseButtonClick()
        {
            OnClose(null);
        }

        private void InitVideoPlayer()
        {
            var vpParent = ContainersGetter.GetContainer(ContainerNames.Common);
            var vpGo = Managers.PrefabSetManager.InitPrefab(
                vpParent, "tutorials", "tutorial_video_player");
            vpGo.transform.SetLocalPosXY(Vector2.left * 1e3f);
            m_VideoPlayer = vpGo.GetCompItem<VideoPlayer>("video_player");
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
        
        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return new[]
                {
                    EInputCommand.ShopPanel,
                    EInputCommand.SettingsPanel
                }
                .Concat(RmazorUtils.MoveAndRotateCommands);
        }

        #endregion
    }
}
using System.Collections;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewUILevelSkippers
{
    public class ViewUILevelSkipperButtonWithAds : ViewUILevelSkipperButtonBase
    {
        #region nonpublic members

        private SpriteRenderer m_Border, m_Background;

        #endregion
        
        #region inject

        private ViewSettings                        ViewSettings                   { get; }
        private IContainersGetter                   ContainersGetter               { get; }
        private IPrefabSetManager                   PrefabSetManager               { get; }
        private IAdsManager                         AdsManager                     { get; }
        private IViewGameTicker                     ViewGameTicker                 { get; }
        private IViewTimePauser                     TimePauser                     { get; }

        private ViewUILevelSkipperButtonWithAds(
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            IContainersGetter                   _ContainersGetter,
            IPrefabSetManager                   _PrefabSetManager,
            IHapticsManager                     _HapticsManager,
            IViewInputTouchProceeder            _TouchProceeder,
            ICameraProvider                     _CameraProvider,
            ILocalizationManager                _LocalizationManager,
            IAdsManager                         _AdsManager,
            IColorProvider                      _ColorProvider,
            IViewGameTicker                     _ViewGameTicker,
            IViewBetweenLevelAdShower           _BetweenLevelAdShower,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IMoneyCounter                       _MoneyCounter,
            IViewInputCommandsProceeder         _CommandsProceeder,
            ILevelsLoader                       _LevelsLoader,
            IViewTimePauser                     _TimePauser) 
            : base(
                _Model,
                _CameraProvider,
                _ColorProvider,
                _CommandsProceeder,
                _BetweenLevelAdShower, 
                _SwitchLevelStageCommandInvoker, 
                _LevelsLoader,
                _MoneyCounter,
                _LocalizationManager,
                _HapticsManager,
                _TouchProceeder)
        {
            ViewSettings                   = _ViewSettings;
            ContainersGetter               = _ContainersGetter;
            PrefabSetManager               = _PrefabSetManager;
            AdsManager                     = _AdsManager;
            ViewGameTicker                 = _ViewGameTicker;
            TimePauser                     = _TimePauser;
        }

        #endregion

        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            Cor.Run(Cor.WaitWhile(() => !Initialized,
                () =>
                {
                    switch (_ColorId)
                    {
                        case ColorIds.UiText:       Text.color       = _Color; break;
                        case ColorIds.UiBorder:     m_Border.color     = _Color; break;
                        case ColorIds.UiBackground: m_Background.color = _Color; break;
                    }
                }));
            base.OnColorChanged(_ColorId, _Color);
        }
        
        protected override void OnActiveCameraChanged(Camera _Camera)
        {
            ButtonObj.SetParent(_Camera.transform);
            var tr = ButtonObj.transform;
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            float yPos = screenBounds.max.y;
            string nextLevelType = (string)Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyNextLevelType, out _);
            yPos = nextLevelType switch
            {
                CommonInputCommandArg.ParameterLevelTypeMain  => yPos - 8f,
                CommonInputCommandArg.ParameterLevelTypeBonus => yPos - 4f,
                _                                             => yPos
            };
            tr.SetLocalPosXY(screenBounds.center.x, yPos);
        }
        
        protected override void OnSkipLevelButtonPressed()
        {
            void OnBeforeAdShown()
            {
                TimePauser.PauseTimeInGame();
            }
            void OnAdClosed()
            {
                ActivateButton(false);
                TimePauser.UnpauseTimeInGame();
            }
            AdsManager.ShowRewardedAd(
                OnBeforeAdShown,
                _OnReward: SkipLevel,
                _OnClosed: OnAdClosed);
        }

        protected override void InitButton()
        {
            var parent = ContainersGetter.GetContainer(ContainerNamesCommon.GameUI);
            var go = PrefabSetManager.InitPrefab(
                parent, CommonPrefabSetNames.UiGame, "skip_level_button");
            m_Border = go.GetCompItem<SpriteRenderer>("border");
            m_Border.color = ColorProvider.GetColor(ColorIds.UiBorder);
            m_Border.sortingOrder = SortingOrders.GameUI + 1;
            m_Background = go.GetCompItem<SpriteRenderer>("background");
            m_Background.color = ColorProvider.GetColor(ColorIds.UiBackground);
            m_Background.sortingOrder = SortingOrders.GameUI;
            go.GetCompItem<SpriteRenderer>("no_ads_icon").sortingOrder = SortingOrders.GameUI + 1;
            InitButtonCore(go);
        }
        
        protected override IEnumerator ShowButtonCountdownCoroutine()
        {
            yield return Cor.Delay(ViewSettings.skipLevelSeconds, 
                ViewGameTicker,
                () =>
            {
                if (AdsManager.RewardedAdReady)
                    ActivateButton(true);
            });
        }

        #endregion
    }
}
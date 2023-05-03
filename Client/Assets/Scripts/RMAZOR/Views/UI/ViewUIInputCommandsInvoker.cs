using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;

namespace RMAZOR.Views.UI
{
    public interface IViewUIInputCommandsInvoker : IInit { }
    
    public class ViewUIInputCommandsInvoker : InitBase, IViewUIInputCommandsInvoker
    {
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IManagersGetter             Managers                { get; }
        private IDialogPanelsSet            DialogPanelsSet         { get; }
        private IDialogViewersController    DialogViewersController { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher      { get; }
        private IViewTimePauser             TimePauser              { get; }

        private ViewUIInputCommandsInvoker(
            IViewInputCommandsProceeder _CommandsProceeder,
            IManagersGetter             _Managers,
            IDialogPanelsSet            _DialogPanelsSet,
            IDialogViewersController    _DialogViewersController,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewTimePauser             _TimePauser)
        {
            CommandsProceeder       = _CommandsProceeder;
            Managers                = _Managers;
            DialogPanelsSet         = _DialogPanelsSet;
            DialogViewersController = _DialogViewersController;
            LevelStageSwitcher      = _LevelStageSwitcher;
            TimePauser              = _TimePauser;
        }

        public override void Init()
        {
            CommandsProceeder.Command += OnCommand;
            base.Init();
        }

        private void OnCommand(EInputCommand _Key, Dictionary<string, object> _Args)
        {
            switch (_Key)
            {
                case EInputCommand.ShopMoneyPanel:
#if YANDEX_GAMES
                    return;
#endif
                    ShowShopPanel<IShopMoneyDialogPanel>(_Args);
                    break;
                case EInputCommand.ShopPanel:
#if YANDEX_GAMES
                    return;
#endif
                    ShowShopPanel<IShopDialogPanel>(_Args);
                    break;
                case EInputCommand.RateGameFromGameUi:
                {
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameMainButtonClick);
                    Managers.ShopManager.RateGame();
                    SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
                }
                    break;
                case EInputCommand.SettingsPanel:
                    ShowDialogPanel<ISettingDialogPanel>(EInputCommand.PauseLevel, _Args);
                    break;
                case EInputCommand.DailyGiftPanel:
                    ShowDialogPanel<IDailyGiftPanel>(EInputCommand.PauseLevel, _Args);
                    break;
                case EInputCommand.LevelsPanel:
                    ShowDialogPanel<ILevelsDialogPanelMain>(EInputCommand.PauseLevel, _Args);
                    break;
                case EInputCommand.PlayBonusLevelPanel:
                    ShowDialogPanel<IPlayBonusLevelDialogPanel>(EInputCommand.PauseLevel, _Args);
                    break;
                case EInputCommand.FinishLevelGroupPanel:
                    ShowDialogPanel<IFinishLevelGroupDialogPanel>(EInputCommand.PauseLevel, _Args);
                    break;
                case EInputCommand.DisableAdsPanel:
                    ShowDialogPanel<IDisableAdsDialogPanel>(EInputCommand.PauseLevel, _Args, 3f);
                    break;
                case EInputCommand.HintPanel:
                    ShowDialogPanel<IHintDialogPanel>(EInputCommand.PauseLevel, _Args, 3f);
                    break;
                case EInputCommand.DailyChallengePanel:
                    ShowDialogPanel<IDailyChallengePanel>(null, _Args);
                    break;
                case EInputCommand.CustomizeCharacterPanel:
                    ShowDialogPanel<ICustomizeCharacterPanel>(null, _Args, 3f);
                    break;
                case EInputCommand.MainMenuPanel:
                    ShowDialogPanel<IMainMenuPanel>(EInputCommand.ExitLevelStaging, _Args);
                    break;
                case EInputCommand.ConfirmGoToMainMenuPanel:
                    ShowDialogPanel<IConfirmGoToMainMenuPanel>(EInputCommand.PauseLevel, _Args);
                    break;
            }
        }

        private void ShowShopPanel<T>(Dictionary<string, object> _Args)
            where T : IDialogPanel, ISetOnCloseFinishAction
        {
            var panel = DialogPanelsSet.GetPanel<T>();
            var dv = DialogViewersController.GetViewer(panel.DialogViewerId);
            panel.SetOnCloseFinishAction(() =>
            {
                object loadShopPanelFromCharDiedPanelArg1 = _Args.GetSafe(
                    ComInComArg.KeyLoadShopPanelFromCharacterDiedPanel, out bool keyExist1);
                if (keyExist1 && (bool)loadShopPanelFromCharDiedPanelArg1)
                {
                    DialogPanelsSet.GetPanel<ICharacterDiedDialogPanel>().ReturnFromShopPanel();
                }
                else
                {
                    TimePauser.UnpauseTimeInGame();
                    LevelStageSwitcher.SwitchLevelStage(
                        EInputCommand.UnPauseLevel);
                }
            });
            dv.Show(panel);
            object loadShopPanelFromCharDiedPanelArg = _Args.GetSafe(
                ComInComArg.KeyLoadShopPanelFromCharacterDiedPanel, out bool keyExist);
            if (!keyExist || (bool)loadShopPanelFromCharDiedPanelArg == false)
                LevelStageSwitcher.SwitchLevelStage(EInputCommand.PauseLevel);
        }

        private void ShowDialogPanel<T>(
            EInputCommand?             _LevelStageCommand,
            Dictionary<string, object> _Args,
            float?                     _AnimationSpeed = null)
            where T : IDialogPanel
        {
            if (_LevelStageCommand.HasValue)
                LevelStageSwitcher.SwitchLevelStage(_LevelStageCommand.Value);
            var panel = DialogPanelsSet.GetPanel<T>();
            var dv = DialogViewersController.GetViewer(panel.DialogViewerId);
            float animationSpeed = _AnimationSpeed ?? 1f;
            var additionalCamEffectAction = (UnityAction<bool, float>)_Args.GetSafe(
                ComInComArg.KeyAdditionalCameraEffectAction, out _);
            dv.Show(panel, animationSpeed, _AdditionalCameraEffectsAction: additionalCamEffectAction);
        }
    }
}
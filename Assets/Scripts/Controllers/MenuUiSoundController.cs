using Constants;
using DialogViewers;
using Settings;
using UI;
using UI.PanelItems;
using UI.Panels;

namespace Controllers
{
    public class MenuUiSoundController : SoundControllerBase
    {
        protected override void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            switch (_Sender)
            {
                case MainMenuUi _:
                    OnNotifyInMainMenuUi(_NotifyMessage);
                    break;
                case SoundSetting _:
                    SwitchSound((bool)_Args[0]);
                    break;
                case MainMenuDialogViewer _:
                case LoginPanel _:
                case RegistrationPanel _:
                case PlusLifesPanel _:
                case PlusMoneyPanel _:
                    PlayUiButtonClick();
                    break;
                case SettingItemInPanelSelector _:
                    // do not play because of duplicating sounds with panel inner selection
                    break;
                default:
                    if (_NotifyMessage == CommonNotifyMessages.UiButtonClick)
                        PlayUiButtonClick();
                    break;
            }
        }

        private void OnNotifyInMainMenuUi(string _NotifyMessage)
        {
            switch (_NotifyMessage)
            {
                case MainMenuUi.NotifyMessageMainMenuLoaded:
                    PlayClip("main_menu_theme", true, 0.1f);
                    break;
                case MainMenuUi.NotifyMessageSelectGamePanelButtonClick:
                case MainMenuUi.NotifyMessageProfileButtonClick:
                case MainMenuUi.NotifyMessageSettingsButtonClick: 
                case MainMenuUi.NotifyMessageLoginButtonClick:
                case MainMenuUi.NotifyMessageShopButtonClick:
                case MainMenuUi.NotifyMessageRatingsButtonClick: 
                case MainMenuUi.NotifyMessageDailyBonusButtonClick: 
                case MainMenuUi.NotifyMessageWheelOfFortuneButtonClick:
                    PlayUiButtonClick();
                    break;
                case MainMenuUi.NotifyMessagePlayButtonClick:
                    StopPlayingClips();
                    break;
            }
        }
        
        private void PlayUiButtonClick()
        {
            PlayClip("ui_button_click", false);
        }
    }
}
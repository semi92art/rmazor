using Common.Entities;
using Common.Enums;

namespace Common.Constants
{
    public static class CommonAudioClipArgs
    {
        public static AudioClipArgs UiButtonClick => new AudioClipArgs(
            "ui_button_click", EAudioClipType.UiSound);
    }
}
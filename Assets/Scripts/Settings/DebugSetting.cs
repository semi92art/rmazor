using System.Collections.Generic;
using Entities;
using UI;
using UI.Managers;
using Utils;

namespace Settings
{
    public class DebugSetting : ISetting
    {
        public string Name => "Debug";
        public SettingType Type => SettingType.OnOff;
        public List<string> Values => null;
        public object Min => null;
        public object Max => null;

        public object Get()
        {
#if DEBUG
            return SaveUtils.GetValue<bool>(SaveKey.SettingDebug);
#endif
            return null;
        }

        public void Put(object _Parameter)
        {
#if DEBUG
            bool debugOn = (bool) _Parameter;
            SaveUtils.PutValue(SaveKey.SettingDebug, debugOn);
            UiManager.Instance.DebugConsole.SetActive(debugOn);
#if !UNITY_EDITOR
            UiManager.Instance.DebugReporter.SetActive(debugOn);
#endif
#endif
        }
    }
}
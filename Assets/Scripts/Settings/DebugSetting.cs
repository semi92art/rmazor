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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return SaveUtils.GetValue<bool>(SaveKeyDebug.DebugUtilsOn);
#endif
            return null;
        }

        public void Put(object _Parameter)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool debugOn = (bool) _Parameter;
            SaveUtils.PutValue(SaveKeyDebug.DebugUtilsOn, debugOn);
            UiManager.Instance.DebugConsole.SetActive(debugOn);
    #if DEVELOPMENT_BUILD
            UiManager.Instance.DebugReporter.SetActive(debugOn);
    #endif 
#endif

        }
    }
}
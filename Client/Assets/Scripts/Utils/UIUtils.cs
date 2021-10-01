using Constants;
using Entities;

namespace Utils
{
    public static class UIUtils
    {
        public static void OnButtonClick(IManagersGetter _Managers, string _AnalyticCode)
        {
            _Managers.Notify(_SM =>
            {
                _SM.PlayClip(AudioClipNames.UIButtonClick);
            }, _AM =>
            {
                _AM.OnAnalytic(_AnalyticCode);
            });
        }
    }
}
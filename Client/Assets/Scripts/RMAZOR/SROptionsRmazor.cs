using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;

namespace RMAZOR
{
    public static class SRLauncher
    {
        public static event UnityAction Initialized;
        
        public static void Init(
            ModelSettings               _ModelSettings,
            ViewSettings                _ViewSettings,
            IModelLevelStaging          _LevelStaging,
            IManagersGetter             _Managers,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            ModelSettings     = _ModelSettings;
            ViewSettings      = _ViewSettings;
            LevelStaging      = _LevelStaging;
            Managers          = _Managers;
            CommandsProceeder = _CommandsProceeder;
            Initialized?.Invoke();
        }
        
        public static ModelSettings               ModelSettings     { get; private set; }
        public static ViewSettings                ViewSettings      { get; private set; }
        public static IModelLevelStaging          LevelStaging      { get; private set; }
        public static IManagersGetter             Managers          { get; private set; }
        public static IViewInputCommandsProceeder CommandsProceeder { get; private set; }
    }
}
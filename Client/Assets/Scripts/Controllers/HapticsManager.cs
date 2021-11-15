using Exceptions;
using Lofelt.NiceVibrations;
using Settings;
using UnityEngine.Events;

namespace Controllers
{

    public enum EHapticsPresetType
    {
        Selection    = 0,
        Success      = 1,
        Warning      = 2,
        Failure      = 3,
        LightImpact  = 4,
        MediumImpact = 5,
        HeavyImpact  = 6,
        RigidImpact  = 7,
        SoftImpact   = 8,
        None         = -1
    }

    public interface IHapticsManager : IInit
    {
        void PlayPreset(EHapticsPresetType _Preset);
        void Play(float _Amplitude, float _Frequency, float _Duration);
    }
    
    public class HapticsManager : IHapticsManager
    {
        #region inject
        
        private IHapticsSetting Setting { get; }

        public HapticsManager(IHapticsSetting _Setting)
        {
            Setting = _Setting;
        }

        #endregion
        
        #region api
        
        public event UnityAction Initialized;
        public void Init()
        {
            HapticController.Init();
            Setting.OnValueSet = EnableHaptics;
            EnableHaptics(Setting.Get());
            Initialized?.Invoke();
        }
        
        public void PlayPreset(EHapticsPresetType _Preset)
        {
            if (!Setting.Get())
                return;
            HapticController.Reset();
            HapticPatterns.PresetType presetType;
            switch (_Preset)
            {
                case EHapticsPresetType.Selection:
                    presetType = HapticPatterns.PresetType.Selection; break;
                case EHapticsPresetType.Success:
                    presetType = HapticPatterns.PresetType.Success; break;
                case EHapticsPresetType.Warning:
                    presetType = HapticPatterns.PresetType.Warning; break;
                case EHapticsPresetType.Failure:
                    presetType = HapticPatterns.PresetType.Failure; break;
                case EHapticsPresetType.LightImpact:
                    presetType = HapticPatterns.PresetType.LightImpact; break;
                case EHapticsPresetType.MediumImpact:
                    presetType = HapticPatterns.PresetType.MediumImpact; break;
                case EHapticsPresetType.HeavyImpact:
                    presetType = HapticPatterns.PresetType.HeavyImpact; break;
                case EHapticsPresetType.RigidImpact:
                    presetType = HapticPatterns.PresetType.RigidImpact; break;
                case EHapticsPresetType.SoftImpact:
                    presetType = HapticPatterns.PresetType.SoftImpact; break;
                case EHapticsPresetType.None:
                    presetType = HapticPatterns.PresetType.None; break;
                default: throw new SwitchCaseNotImplementedException(_Preset);
            }
            HapticPatterns.PlayPreset(presetType);
        }

        public void Play(float _Amplitude, float _Frequency, float _Duration)
        {
            if (!Setting.Get())
                return;
            HapticPatterns.PlayConstant(_Amplitude, _Frequency, _Duration);
        }
        
        #endregion

        #region nonpublic methods

        private static void EnableHaptics(bool _Enable)
        {
            HapticController.hapticsEnabled = _Enable;
        }

        #endregion
    }
}
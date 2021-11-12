using Exceptions;
using UnityEngine;
using Lofelt.NiceVibrations;
using Utils;

namespace Controllers
{
    public enum EVibrationPreset
    {
        CharacterMove,
        CharacterDeath,
        TurretShoot,
        TrapIncreasingOpen,
        FinishLevel
    }
    
    public enum EVibrationPower
    {
        VeryLow,
        Low,
        Medium,
        High
    }

    public interface IVibrationManager
    {
        void PlayPreset(EVibrationPreset _Preset);
        void Play(EVibrationPower _Power, float? _Seconds = null);
    }
    
    public class VibrationManager : IVibrationManager
    {
        public void PlayPreset(EVibrationPreset _Preset)
        {
            if (!IsHapticSupported())
                Dbg.LogWarning("Haptic is not supported!!!");
            else
            {
                HapticPatterns.PresetType? presetType = null;
                switch (_Preset)
                {
                    case EVibrationPreset.CharacterMove:
                        presetType = HapticPatterns.PresetType.LightImpact;
                        // Play(EVibrationPower.Low,  0.01f);
                        break;
                    case EVibrationPreset.CharacterDeath:
                        presetType = HapticPatterns.PresetType.Failure;
                        break;
                    case EVibrationPreset.TrapIncreasingOpen:
                    case EVibrationPreset.TurretShoot:
                        presetType = HapticPatterns.PresetType.LightImpact;
                        break;
                    case EVibrationPreset.FinishLevel:
                        presetType = HapticPatterns.PresetType.None;
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(_Preset);
                }
                if (presetType.HasValue)
                    HapticPatterns.PlayPreset(presetType.Value);
            }
        }

        public void Play(EVibrationPower _Power, float? _Seconds = null)
        {
            // if (!IsHapticSupported())
            // {
            //     Handheld.Vibrate();
            //     return;
            // }
            float secs = _Seconds ?? 0.05f;
            float amplitude = GetAmplitude(_Power);
            HapticPatterns.PlayConstant(amplitude, 1f, secs);
        }

        private float GetAmplitude(EVibrationPower _Power)
        {
            switch (_Power)
            {
                case EVibrationPower.VeryLow:
                    return 0.01f;
                case EVibrationPower.Low:
                    return 0.33f;
                case EVibrationPower.Medium:
                    return 0.66f;
                case EVibrationPower.High:
                    return 1f;
                default:
                    throw new SwitchCaseNotImplementedException(_Power);
            }
        }

        private static HapticPatterns.PresetType GetPresetType(EVibrationPreset _Preset)
        {
            switch (_Preset)
            {
                case EVibrationPreset.CharacterMove:
                    return HapticPatterns.PresetType.SoftImpact;
                case EVibrationPreset.CharacterDeath:
                    return HapticPatterns.PresetType.Failure;
                case EVibrationPreset.TrapIncreasingOpen:
                case EVibrationPreset.TurretShoot:
                    return HapticPatterns.PresetType.LightImpact;
                case EVibrationPreset.FinishLevel:
                    return HapticPatterns.PresetType.None;
                default:
                    throw new SwitchCaseNotImplementedException(_Preset);
            }
        }

        private static bool IsHapticSupported()
        {
            return DeviceCapabilities.meetsAdvancedRequirements;
        }
    }
}
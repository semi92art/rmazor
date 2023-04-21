using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;

namespace Common.Managers
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
        
        void Play(float _Amplitude, float _Frequency, float? _Duration = null);
    }

    public class HapticsManagerFake : InitBase, IHapticsManager
    {
        public void PlayPreset(EHapticsPresetType _Preset) { }

        public void Play(float _Amplitude, float _Frequency, float? _Duration = null) { }
    }
}
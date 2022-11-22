using System;
using UnityEngine;
using UnityEngine.Events;

namespace Common.CameraProviders
{
    public enum ECameraEffect
    {
        DepthOfField,
        Glitch,
        ColorGrading,
        ChromaticAberration,
        AntiAliasing,
        Pixelate,
        Bloom
    }
    
    public interface ICameraEffectProps { }

    public interface ICameraProvider : IInit
    {
        event UnityAction<Camera> ActiveCameraChanged;  
        Func<Bounds> GetMazeBounds     { set; }
        Func<float>  GetConverterScale { set; }
        Transform    Follow            { set; }
        Camera       Camera            { get; }
        void         SetEffectProps<T>(ECameraEffect     _Effect, T _Args) where T : ICameraEffectProps;
        void         AnimateEffectProps<T>(ECameraEffect _Effect, T _From, T _To, float _Duration) where T : ICameraEffectProps;
        bool         IsEffectEnabled(ECameraEffect       _Effect);
        void         EnableEffect(ECameraEffect          _Effect, bool _Enabled);
        void         UpdateState();
    }
}
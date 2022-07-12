using System;
using UnityEngine;

namespace Common.CameraProviders
{
    public enum ECameraEffect
    {
        DepthOfField,
        Glitch,
        ColorGrading,
        ChromaticAberration,
        AntiAliasing,
        Pixelate
    }
    
    public interface ICameraEffectProps { }

    public interface ICameraProvider : IInit
    {
        Func<Bounds> GetMazeBounds     { set; }
        Func<float>  GetConverterScale { set; }
        Transform    Follow            { set; }
        Camera       Camera            { get; }
        void         SetEffectParameters<T>(ECameraEffect _Effect, T _Args) where T : ICameraEffectProps;
        bool         IsEffectEnabled(ECameraEffect        _Effect);
        void         EnableEffect(ECameraEffect           _Effect, bool _Enabled);
        void         UpdateState();
    }
    
}
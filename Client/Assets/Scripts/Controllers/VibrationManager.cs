using UnityEngine;
using Lofelt.NiceVibrations;

// TODO
namespace Controllers
{
    public enum EVibrationType
    {
        Simple,
    }

    public interface IVibrationManager
    {
        void Vibrate();
    }
    
    public class VibrationManager : IVibrationManager
    {
        public void Vibrate()
        {
            //TODO
            // HapticPatterns.PlayEmphasis(0.1f, );
        }
    }
}
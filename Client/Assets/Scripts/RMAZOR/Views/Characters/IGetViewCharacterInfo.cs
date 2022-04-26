using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public struct ViewCharacterInfo
    {
        public Transform    Transform { get; }
        public Collider2D[] Colliders { get; }
        
        public ViewCharacterInfo(Transform _Transform, Collider2D[] _Colliders)
        {
            Transform = _Transform;
            Colliders = _Colliders;
        }

    }
    
    public interface IGetViewCharacterInfo
    {
        System.Func<ViewCharacterInfo> GetViewCharacterInfo { set; }
    }
}
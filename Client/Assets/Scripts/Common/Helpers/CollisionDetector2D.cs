using UnityEngine;
using UnityEngine.Events;

namespace Common.Helpers
{
    public class CollisionDetector2D : MonoBehaviour
    {
        public UnityAction<Collider2D> OnTriggerEnter { get; set; }
        public UnityAction<Collider2D> OnTriggerStay { get; set; }
        public UnityAction<Collider2D> OnTriggerExit { get; set; }
        
        public void OnTriggerEnter2D(Collider2D _Collider)
        {
            OnTriggerEnter?.Invoke(_Collider);
        }
        
        public void OnTriggerStay2D(Collider2D _Collider)
        {
            OnTriggerStay?.Invoke(_Collider);
        }
        
        public void OnTriggerExit2D(Collider2D _Collider)
        {
            OnTriggerExit?.Invoke(_Collider);
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameHelpers
{
    [RequireComponent(typeof(Collider2D))]
    public class Collider2DActionsHandler : MonoBehaviour
    {
        [Serializable]
        public class Collider2DEvent : UnityEvent<Collider2D>{ }
        [Serializable]
        public class Collision2DEvent : UnityEvent<Collision2D>{ }
        
        [SerializeField] public Collider2DEvent triggerEnterAction;
        [SerializeField] public Collider2DEvent triggerExitAction;
        [SerializeField] public Collider2DEvent triggerStayAction;

        [SerializeField] public Collision2DEvent collisionEnterAction;
        [SerializeField] public Collision2DEvent collisionExitAction;
        [SerializeField] public Collision2DEvent collisionStayAction;
        
        public void OnTriggerEnter2D(Collider2D _C) => triggerEnterAction?.Invoke(_C);
        public void OnTriggerExit2D(Collider2D _C) => triggerExitAction?.Invoke(_C);
        public void OnTriggerStay2D(Collider2D _C) => triggerStayAction?.Invoke(_C);
        
        public void OnCollisionEnter2D(Collision2D _C) => collisionEnterAction?.Invoke(_C);
        public void OnCollisionExit2D(Collision2D _C) => collisionExitAction?.Invoke(_C);
        public void OnCollisionStay2D(Collision2D _C) => collisionStayAction?.Invoke(_C);
    }
}
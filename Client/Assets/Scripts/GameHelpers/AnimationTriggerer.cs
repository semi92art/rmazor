using UnityEngine;
using UnityEngine.Events;

namespace GameHelpers
{
    public class AnimationTriggerer : MonoBehaviour
    {
        public UnityAction Trigger1;
        public UnityAction Trigger2;
        public UnityAction Trigger3;

        public void RaiseTrigger1() => Trigger1?.Invoke();
        public void RaiseTrigger2() => Trigger2?.Invoke();
        public void RaiseTrigger3() => Trigger3?.Invoke();
    }
}
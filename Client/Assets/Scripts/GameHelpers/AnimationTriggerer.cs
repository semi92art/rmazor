using UnityEngine;
using UnityEngine.Events;

namespace GameHelpers
{
    public class AnimationTriggerer : MonoBehaviour
    {
        public UnityAction Trigger1;
        public UnityAction Trigger2;
        public UnityAction Trigger3;
        public UnityAction Trigger4;
        public UnityAction Trigger5;
        public UnityAction Trigger6;
        public UnityAction Trigger7;
        public UnityAction Trigger8;
        public UnityAction Trigger9;
        public UnityAction Trigger10;

        public void RaiseTrigger1() => Trigger1?.Invoke();
        public void RaiseTrigger2() => Trigger2?.Invoke();
        public void RaiseTrigger3() => Trigger3?.Invoke();
        public void RaiseTrigger4() => Trigger4?.Invoke();
        public void RaiseTrigger5() => Trigger5?.Invoke();
        public void RaiseTrigger6() => Trigger6?.Invoke();
        public void RaiseTrigger7() => Trigger7?.Invoke();
        public void RaiseTrigger8() => Trigger8?.Invoke();
        public void RaiseTrigger9() => Trigger9?.Invoke();
        public void RaiseTrigger10() => Trigger10?.Invoke();
    }
}
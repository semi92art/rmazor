using UnityEngine;

namespace GameHelpers
{
    public class AnimationTriggerer : MonoBehaviour
    {
        public NoArgsHandler Trigger1;
        public NoArgsHandler Trigger2;

        public void RaiseTrigger1() => Trigger1?.Invoke();

        public void RaiseTrigger2() => Trigger2?.Invoke();
    }
}
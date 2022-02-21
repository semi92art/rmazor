using UnityEngine;

namespace RMAZOR.Helpers
{
    public class AnimationEventCounter : MonoBehaviour
    {
        public int count;

        public void OnEvent()
        {
            count++;
        }
    }
}

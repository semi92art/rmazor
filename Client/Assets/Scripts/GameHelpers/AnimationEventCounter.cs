using UnityEngine;

namespace GameHelpers
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

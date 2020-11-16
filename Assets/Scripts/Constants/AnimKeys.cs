using UnityEngine;

namespace Constants
{
    public static class AnimKeys
    {
        public static int Anim { get; }
        public static int Anim2 { get; }
        public static int Anim3 { get; }
        public static int Anim4 { get; }
        public static int Stop { get; }
        public static int Stop2 { get; }
        public static int Normal { get; }
        public static int Selected { get; }

        static AnimKeys()
        {
            Anim = Animator.StringToHash("anim");
            Anim2 = Animator.StringToHash("anim2");
            Anim3 = Animator.StringToHash("anim3");
            Anim4 = Animator.StringToHash("anim4");
            Stop = Animator.StringToHash("stop");
            Stop2 = Animator.StringToHash("stop2");
            Normal = Animator.StringToHash("Normal");
            Selected = Animator.StringToHash("Selected");
        }
    }
}
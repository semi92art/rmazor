using UnityEngine;

public static class AnimKeys
{
    public static int Anim { get; }
    public static int Stop { get; }

    static AnimKeys()
    {
        Anim = Animator.StringToHash("anim");
        Stop = Animator.StringToHash("stop");
    }
}
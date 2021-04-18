using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventCounter : MonoBehaviour
{
    public int count;

    public void OnEvent()
    {
        count++;
    }
}

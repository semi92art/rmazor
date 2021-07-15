using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RotTest : MonoBehaviour
{
    public Transform tr;
    public Transform t2;
    public Transform pt;
    public bool rot;
    public bool sete;
    public Vector3 axis;
    public Vector3 eulerAngles;
    public float angle;

    private void Update()
    {
        if (sete)
        {
            SetEulerAngles();
            //sete = false;
        }
        
        if (!rot)
            return;
        
        tr.RotateAround(pt.position, axis, angle);
    }

    private void SetEulerAngles()
    {
        t2.eulerAngles = eulerAngles;
    }
}

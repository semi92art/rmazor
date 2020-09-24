using System;
using UnityEditor;
using UnityEngine;
using Network;
using Network.PacketArgs;
using Network.Packets;

public class EditorHelper : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(EditorHelper))]
public class EditorHelperEditor : Editor
{
    private EditorHelper m_Helper;
    
    private void OnEnable()
    {
        m_Helper = target as EditorHelper;
    }

    public override void OnInspectorGUI()
    {

    }
}

#endif

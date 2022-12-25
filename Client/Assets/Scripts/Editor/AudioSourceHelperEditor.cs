// ReSharper disable CheckNamespace
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime.Utils;
using UnityEditor;

[CustomEditor(typeof(AudioSourceHelper))]
public class AudioSourceHelperEditor : UnityEditor.Editor
{
    private AudioSourceHelper m_O;

    private void OnEnable()
    {
        m_O = target as AudioSourceHelper;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorUtilsEx.GuiButtonAction(PlayClip);
    }

    private void PlayClip()
    {
        m_O.source.Play();
    }
}

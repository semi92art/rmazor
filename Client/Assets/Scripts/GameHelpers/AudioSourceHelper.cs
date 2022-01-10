using UnityEngine;

public class AudioSourceHelper : MonoBehaviour
{
    public AudioSource source;

    private void OnEnable()
    {
        source = GetComponent<AudioSource>();
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(AudioSourceHelper))]
public class AudioSourceHelperEditor : UnityEditor.Editor
{
    private AudioSourceHelper o;

    private void OnEnable()
    {
        o = target as AudioSourceHelper;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Utils.EditorUtilsEx.GuiButtonAction(PlayClip);
    }

    private void PlayClip()
    {
        o.source.Play();
    }
}
#endif

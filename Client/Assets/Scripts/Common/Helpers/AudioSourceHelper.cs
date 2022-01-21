using UnityEngine;

namespace Common.Helpers
{
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
        private AudioSourceHelper m_O;

        private void OnEnable()
        {
            m_O = target as AudioSourceHelper;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Utils.EditorUtilsEx.GuiButtonAction(PlayClip);
        }

        private void PlayClip()
        {
            m_O.source.Play();
        }
    }
#endif
}
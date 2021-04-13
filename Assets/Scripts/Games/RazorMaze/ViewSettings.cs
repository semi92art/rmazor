using UnityEngine;

namespace Games.RazorMaze
{
    [CreateAssetMenu(fileName = "view_settings", menuName = "View Settings", order = 1)]
    public class ViewSettings : ScriptableObject
    {
        [SerializeField] private float lineWidth;
        [SerializeField] private float cornerRadius;

        public float LineWidth => lineWidth * 0.01f;
        public float CornerRadius => cornerRadius * 0.01f;
    }
}
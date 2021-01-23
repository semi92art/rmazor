using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PrototypingUiDebugItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button button;

        public void Init(string _Title, Button.ButtonClickedEvent _Event)
        {
            gameObject.name = _Title;
            title.text = _Title;
            button.onClick = _Event;
        }
    }
}
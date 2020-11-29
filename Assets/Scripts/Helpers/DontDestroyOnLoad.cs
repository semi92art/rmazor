using Network;
using UnityEngine;

namespace Helpers
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake()
        {
            if (!GameClient.Instance.IsTestMode)
                DontDestroyOnLoad(gameObject);
        }
    }
}

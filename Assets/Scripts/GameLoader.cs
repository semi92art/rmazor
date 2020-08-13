using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clickers
{
    public class GameLoader : MonoBehaviour
    {
        private void Start()
        {
            GoogleAdsManager.Instance.Init();
            SceneManager.LoadScene("Main");

            SceneManager.activeSceneChanged += (previous, current) => { };
        }
    }
}
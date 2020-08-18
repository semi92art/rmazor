using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clickers
{
    public class GameLoader : MonoBehaviour
    {
        public bool isAddsOn = false;
        public string SceneToLoad = "_menu";
        private void Start()
        {
            if (this.isAddsOn)
                GoogleAdsManager.Instance.Init();
            SceneManager.LoadScene(this.SceneToLoad);

            SceneManager.activeSceneChanged += (previous, current) => { };
        }
    }
}
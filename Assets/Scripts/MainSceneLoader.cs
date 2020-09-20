using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace  Clickers
{
    public class MainSceneLoader : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(CheckInternetConnection());
            StartCoroutine(CheckMainServerConnection());
        }

        private IEnumerator CheckInternetConnection()
        {
            if (!CommonUtils.CheckForInternetConnection())
                LoadInternetFailDialog();
            yield return null;
        }

        private IEnumerator CheckMainServerConnection()
        {
            if (!CommonUtils.CheckForMainServerInternetConnection())
                LoadInternetFailDialog();
            yield return null;
        }
    
        private void LoadInternetFailDialog()
        {
        }

        private void LoadChooseSetPanel()
        {
        
        }

        private void LoadClickingPanel()
        {
        
        }

        private void LoadSendAddressPanel()
        {
        
        }
    }
}


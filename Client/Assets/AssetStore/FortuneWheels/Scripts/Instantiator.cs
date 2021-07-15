using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mkey;
using UnityEngine.UI;

namespace MkeyFW
{
    public class Instantiator : MonoBehaviour
    {
        public GameObject[] prefabs;
        public EaseAnim ease;
        private int current = 0;
        private GameObject currentGo;
        private Button button;

        private void Start()
        {
            CreateNext();
        }

        public void CreateNext()
        {
            button = GetComponent<Button>();
            if (prefabs == null || prefabs.Length == 0) return;
            if (prefabs[current])
            {
                if (button) button.interactable = false;
                if (currentGo) Destroy(currentGo);
                currentGo = Instantiate(prefabs[current]);
                SimpleTween.Value(gameObject, 0, 1, 0.25f)
                    .SetOnUpdate((float val) => { currentGo.transform.localScale = new Vector3(val, val, val); })
                    .SetEase(ease);
    

                SimpleTween.Value(gameObject, 0, 1, 0.5f).AddCompleteCallBack(() =>
                {
                    if (button) button.interactable = true;
                    currentGo.GetComponent<WheelController>().StartSpin();
                });
            }
            current++;
            if (current >= prefabs.Length) current = 0;
        }
    }
}
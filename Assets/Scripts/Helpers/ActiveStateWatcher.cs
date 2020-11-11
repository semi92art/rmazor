using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helpers
{
    public class ActiveStateWatcher : MonoBehaviour
    {
        private Dictionary<GameObject,AudioSource> m_clipDictionary = new Dictionary<GameObject, AudioSource>();

        public event ActiveStateHandler ActiveStateChanged;

        private void CheckPlayingClips()
        {
            var array = m_clipDictionary.ToArray();

            foreach (var item in array)
            {
                if (item.Key != null && !item.Value.isPlaying)
                {
                    //remove
                }
            }
        }
        

        private void OnEnable()
        {
            ActiveStateChanged?.Invoke(new ActiveStateEventArgs(true));
        }

        private void OnDisable()
        {
            ActiveStateChanged?.Invoke(new ActiveStateEventArgs(false));
        }
    }

    public delegate void ActiveStateHandler(ActiveStateEventArgs _Args);

    public class ActiveStateEventArgs
    {
        public bool IsActive { get; }

        public ActiveStateEventArgs(bool _IsActive)
        {
            IsActive = _IsActive;
        }
    }
}
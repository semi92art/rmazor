using System.Collections.Generic;
using DI.Extensions;
using Entities;
using GameHelpers;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Games.RazorMaze.Views.Common
{
    public interface IColorProvider : IInit
    {
        Color                           GetColor(int _Id);
        void                            SetColor(int _Id, Color _Color);
        event UnityAction<int, Color>   ColorChanged;
        IReadOnlyDictionary<int, Color> Colors { get; }
    }
    
    public class DefaultColorProvider : MonoBehaviour, IColorProvider
    {
        #region nonpublic members

        private readonly Dictionary<int, Color>                m_ColorsDict = new Dictionary<int, Color>();
        private          ColorSetScriptableObject.ColorItemSet m_Set;

        #endregion

        #region inject
        
        private IPrefabSetManager PrefabSetManager { get; set; }

        [Inject]
        public void Inject(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api
        
        public event UnityAction<int, Color>   ColorChanged;
        public IReadOnlyDictionary<int, Color> Colors      => m_ColorsDict;
        public bool                            Initialized { get; private set; }
        public event UnityAction               Initialize;
        
        public void Init()
        {
            m_Set = PrefabSetManager.GetObject<ColorSetScriptableObject>(
                "views", "color_set").set;
            foreach (var item in m_Set)
                m_ColorsDict.Add(ColorIds.GetHash(item.name), item.color);
            Initialize?.Invoke();
            Initialized = true;
        }

        public Color GetColor(int _Id)
        {
            if (m_ColorsDict.ContainsKey(_Id)) 
                return m_ColorsDict[_Id];
            Dbg.LogWarning($"Color \"{ColorIds.GetColorNameById(_Id)}\" with key \"{_Id}\" was not set.");
            return default;
        }

        public void SetColor(int _Id, Color _Color)
        {
            m_ColorsDict.SetSafe(_Id, _Color);
            ColorChanged?.Invoke(_Id, _Color);
        }
        
        #endregion
    }
}
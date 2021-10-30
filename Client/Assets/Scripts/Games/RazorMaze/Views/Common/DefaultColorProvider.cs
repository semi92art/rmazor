using System.Collections.Generic;
using Entities;
using GameHelpers;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Common
{
    public interface IColorProvider : IInit
    {
        Color                         GetColor(int _Id);
        void                          SetColor(int _Id, Color _Color);
        event UnityAction<int, Color> ColorChanged;
    }
    
    public class DefaultColorProvider : MonoBehaviour, IColorProvider
    {
        #region nonpublic members

        private readonly Dictionary<int, Color>                m_ColorsDict = new Dictionary<int, Color>();
        private          ColorSetScriptableObject.ColorItemSet m_Set;

        #endregion

        #region api
        
        public event UnityAction<int, Color> ColorChanged;
        public event UnityAction             Initialized;
        
        public void Init()
        {
            m_Set = PrefabUtilsEx.GetObject<ColorSetScriptableObject>(
                "views", "color_set").set;
            foreach (var item in m_Set)
                m_ColorsDict.Add(ColorIds.GetHash(item.name), item.color);
        }

        public Color GetColor(int _Id)
        {
            if (m_ColorsDict.ContainsKey(_Id)) 
                return m_ColorsDict[_Id];
            Dbg.LogWarning($"Color with key {_Id} was not set.");
            return default;
        }

        public void SetColor(int _Id, Color _Color)
        {
            if (m_ColorsDict.ContainsKey(_Id))
            {
                m_ColorsDict[_Id] = _Color;
                ColorChanged?.Invoke(_Id, _Color);
            }
            else m_ColorsDict.Add(_Id, _Color);
        }
        
        #endregion
    }
}
using System.Collections.Generic;
using DI.Extensions;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public class ContainersGetter : IContainersGetter
    {
        #region nonpublic members
        
        private readonly Dictionary<string, Transform> m_Containers = new Dictionary<string, Transform>();
        private readonly Dictionary<string, bool> m_Initialized = new Dictionary<string, bool>();
        
        #endregion
        
        #region inject

        private ICoordinateConverter CoordinateConverter { get; }

        public ContainersGetter(ICoordinateConverter _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion

        #region api

        public Transform MazeContainer => GetContainer("Maze");
        public Transform MazeItemsContainer => GetContainer("Maze Items", true);
        public Transform CharacterContainer => GetContainer("Character", true);
        public Transform BackgroundContainer => GetContainer("Background");
        public Transform AudioSourcesContainer => GetContainer("AudioSources");

        #endregion

        #region nonpublic methods

        private Transform GetContainer(string _Name, bool _InMazeContainer = false)
        {
            if (!m_Initialized.ContainsKey(_Name))
            {
                m_Initialized.Add(_Name, false);
                m_Containers.Add(_Name, null);
            }
            if (m_Initialized[_Name])
                return m_Containers[_Name];
            m_Containers[_Name] = CommonUtils.FindOrCreateGameObject(_Name, out _).transform;
            m_Containers[_Name].SetParent(_InMazeContainer ? MazeContainer : null);
            m_Containers[_Name].SetPosXY(CoordinateConverter.GetCenter());
            m_Initialized[_Name] = true;
            return m_Containers[_Name];
        }
        
        #endregion

    }
}
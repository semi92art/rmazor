using DI.Extensions;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public class ContainersGetter : IContainersGetter
    {
        #region nonpublic members

        private Transform m_MazeContainer;
        private Transform m_MazeItemsContainer;
        private Transform m_CharacterContainer;
        private Transform m_BackgroundContainer;
        
        private bool m_MazeContainerInitialized;
        private bool m_MazeItemsContainerInitialized;
        private bool m_CharacterContainerInitialized;
        private bool m_BackgroundContainerInitialized;
        
        #endregion
        
        #region inject
        
        private ICoordinateConverter CoordinateConverter { get; }

        public ContainersGetter(ICoordinateConverter _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion

        #region api
        
        public Transform MazeContainer
        {
            get
            {
                if (m_MazeContainerInitialized) 
                    return m_MazeContainer;
                m_MazeContainer = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
                m_MazeContainer.SetPosXY(CoordinateConverter.GetCenter());
                m_MazeContainerInitialized = true;
                return m_MazeContainer;
            }
        }

        public Transform MazeItemsContainer
        {
            get
            {
                if (m_MazeItemsContainerInitialized)
                    return m_MazeItemsContainer;
                m_MazeItemsContainer = CommonUtils.FindOrCreateGameObject("Maze Items", out _).transform;
                m_MazeItemsContainer.SetParent(MazeContainer);
                m_MazeItemsContainer.SetPosXY(CoordinateConverter.GetCenter());
                m_MazeItemsContainerInitialized = true;
                return m_MazeItemsContainer;
            }
        }

        public Transform CharacterContainer
        {
            get
            {
                if (m_CharacterContainerInitialized)
                    return m_CharacterContainer;
                m_CharacterContainer = CommonUtils.FindOrCreateGameObject("Character", out _).transform;
                m_CharacterContainer.SetParent(MazeContainer);
                m_CharacterContainerInitialized = true;
                return m_CharacterContainer;
            }
        }

        public Transform BackgroundContainer
        {
            get
            {
                if (m_BackgroundContainerInitialized)
                    return m_BackgroundContainer;
                m_BackgroundContainer = CommonUtils.FindOrCreateGameObject("Background", out _).transform;
                m_BackgroundContainer.SetParent(null);
                m_BackgroundContainerInitialized = true;
                return m_BackgroundContainer;
            }
        }

        #endregion

        

    }
}
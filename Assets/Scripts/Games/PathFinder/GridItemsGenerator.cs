using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace Games.PathFinder
{
    public interface IGridItemsGenerator
    {
        Dictionary<Point, TileBehaviour> Board { get; set; }
        int GridWidthInHexes { get; set; }
        int GridHeightInHexes { get; set; }
        void GenerateItems();
        bool DoInstantiate { get; set; }
        void ClearGrid();
    }
    
    public interface IOnLevelStartedFinished
    {
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(LevelStateChangedArgs _Args);
    }

    public class GridItemsGenerator : DI.DiObject, IGridItemsGenerator, IOnLevelStartedFinished
    {
        #region public properties

        public Dictionary<Point, TileBehaviour> Board { get; set; }
        public int GridHeightInHexes { get; set; }
        public int GridWidthInHexes { get; set; }

        public bool DoInstantiate { get; set; }
        
        #endregion
        
        #region nonpublic members
        private float m_CurrentTime;
        private float m_CurrentLevel;
        private bool m_IsLevelInProcess;
        private float m_hexWidth = 4f;
        private float m_hexHeight = 4f;
        private int m_XLim;
        #endregion
        
        #region api

        public GridItemsGenerator()
        {
            GridHeightInHexes = 11;
            GridWidthInHexes = 5;
        }

        public void GenerateItems()
        {
            if (!DoInstantiate)
                return;
            
            if (!m_IsLevelInProcess)
                return;
            
            Vector2 gridSize = new Vector2(GridWidthInHexes, GridHeightInHexes);
            GameObject hexGridGameObject = new GameObject("HexGrid");
            Board = new Dictionary<Point, TileBehaviour>();
            
            for (float y = 0; y < this.GridHeightInHexes; y++)
            {
                
                if (y % 2 == 0)
                {
                    m_XLim = GridWidthInHexes;
                }
                else
                {
                    m_XLim = GridWidthInHexes - 1;
                }
                for (float x = 0; x < m_XLim; x++)
                {
                    
                    string hexName = String.Format("Hex_{0}_{1}",x,y);
                    GameObject hex = new GameObject(hexName);
                    
                    //current pos
                    Vector2 gridPos = new Vector2(x, y);
                    hex.transform.position = CalcWorldCoord(gridPos);
                    hex.transform.parent = hexGridGameObject.transform;
                    hex.transform.localScale = new Vector3(m_hexWidth,m_hexHeight,0f);

                    PolygonCollider2D collider = hex.AddComponent<PolygonCollider2D>();
                    collider.SetPath(0, new []
                    {
                        new Vector2(0f,1f),
                        new Vector2(-0.9f,0.5f),
                        new Vector2(-0.9f,-0.5f),
                        new Vector2(0f,-1f),
                        new Vector2(0.9f,-0.5f),
                        new Vector2(0.9f,0.5f)
                    });
                    //set beauty
                    RegularPolygon regularPolygon = hex.AddComponent<RegularPolygon>();
                    regularPolygon.Color = new Color(138f/255f,135f/255f,221f/255f);
                    regularPolygon.Sides = 6;
                    regularPolygon.Roundness = 0.3f;
                
                    TileBehaviour tileBehaviour = hex.AddComponent<TileBehaviour>();
                    tileBehaviour.tile = new Tile((int)x, (int)y);
                    Board.Add(tileBehaviour.tile.Location, tileBehaviour);
                }
            }
            int counter = 0;
            //Neighboring tile coordinates of all the tiles are calculated
            try
            {
                foreach (TileBehaviour tb in Board.Values)
                {
                    //TODO: поправить для четных строк, где число клеток на 1 меньше, чем в нечетных
                    tb.tile.FindNeighbours(Board, new Vector2(GridWidthInHexes, GridHeightInHexes), true);
                    counter++;
                }
            }
            catch (SystemException ex)
            {
                Debug.Log(ex.Message);
                Debug.Log(counter);
            }
            Debug.Log("Grid: done");
        }
        
        public void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            m_CurrentLevel = _Args.Level;
            m_IsLevelInProcess = true;
            GenerateItems();
        }

        public void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            m_IsLevelInProcess = false;
        }

        public void ClearGrid()
        {
            Board.Clear();
        }
        #endregion

        #region nonpublic methods

        private Vector3 CalcWorldCoord(Vector2 _GridPos)
            {
                //position of first hex
                Vector3 initPos = this.CalulateInitPos();
                float offset = 0;
                if (_GridPos.y % 2 != 0)
                {
                    offset = m_hexWidth /2 ;
                }
        
                float x = initPos.x + (offset * 1.75f)+ _GridPos.x * m_hexWidth * 1.75f;
                float y = initPos.z - _GridPos.y * m_hexHeight * 1.55f;
        
                return new Vector3(x, y, 0);
            }
        
        private Vector3 CalulateInitPos()
        {
            Vector3 initPos = new Vector3(-this.m_hexWidth * this.GridWidthInHexes / 2f - this.m_hexWidth, 0,
                this.GridHeightInHexes / 2f * this.m_hexHeight + this.m_hexHeight / 2 + 6);
            return initPos;
        }

        #endregion
    }
}

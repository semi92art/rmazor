using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace Games.PathFinder
{
    public interface IGridItemsGenerator
    {
        Dictionary<Point, TileBehaviour> Board { get; set; }
        int gridWidthInHexes { get; set; }
        int gridHeightInHexes { get; set; }
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
        public int gridHeightInHexes { get; set; }
        public int gridWidthInHexes { get; set; }

        public bool DoInstantiate { get; set; }
        
        #endregion
        
        #region nonpublic members
        private float m_CurrentTime;
        private float m_CurrentLevel;
        private bool m_IsLevelInProcess;
        private float m_hexWidth = 1f;
        private float m_hexHeight = 1f;
        #endregion
        
        #region api

        public GridItemsGenerator()
        {
            gridHeightInHexes = 4;
            gridWidthInHexes = 5;
        }

        public void GenerateItems()
        {
            if (!DoInstantiate)
                return;
            
            if (!m_IsLevelInProcess)
                return;
            
            Vector2 gridSize = new Vector2(gridWidthInHexes, gridHeightInHexes);
            GameObject hexGridGameObject = new GameObject("HexGrid");
            Board = new Dictionary<Point, TileBehaviour>();
            
            for (float y = 0; y < this.gridHeightInHexes; y++)
            {
                for (float x = 0; x < this.gridWidthInHexes; x++)
                {
                    string hexName = String.Format("Hex_{0}_{1}",x,y);
                    GameObject hex = new GameObject(hexName);
                    //current pos
                    Vector2 gridPos = new Vector2(x, y);
                    hex.transform.position = CalcWorldCoord(gridPos);
                    hex.transform.parent = hexGridGameObject.transform;
                    hex.transform.localScale = new Vector3(m_hexWidth,m_hexHeight,0f);
                
                    MeshCollider meshCollider = hex.AddComponent<MeshCollider>();
                    meshCollider.material = Resources.Load("materials/PathFinder/PMatHex", typeof(PhysicMaterial)) as PhysicMaterial;
                    meshCollider.sharedMesh = Resources.Load("meshes/PathFinder/Hex", typeof(Mesh)) as Mesh;
                
                    //set beauty
                    RegularPolygon regularPolygon = hex.AddComponent<RegularPolygon>();
                    regularPolygon.Color = new Color(231f/255f,255f/255f,170f/255f);
                    regularPolygon.Sides = 6;
                
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
                    tb.tile.FindNeighbours(Board, new Vector2(gridWidthInHexes, gridHeightInHexes), true);
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
        
                //float x = initPos.x + offset + _GridPos.x * m_hexWidth;
                float x = initPos.x + (offset* 1.75f)+ _GridPos.x*m_hexWidth* 1.75f;
                //float y = initPos.z - _GridPos.y * m_hexHeight * 1.75f;
                float y = initPos.z - _GridPos.y * m_hexHeight * 1.55f;
        
                return new Vector3(x, y, 0);
            }
        
        private Vector3 CalulateInitPos()
        {
            Vector3 initPos = new Vector3(-this.m_hexWidth * this.gridWidthInHexes / 2f + this.m_hexWidth / 2, 0,
                this.gridHeightInHexes / 2f * this.m_hexHeight - this.m_hexHeight / 2);
            return initPos;
        }

        #endregion
    }
}

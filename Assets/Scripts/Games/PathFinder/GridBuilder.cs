using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

namespace Games.PathFinder
{
    public class GridBuilder : MonoBehaviour
    {
        public GameObject line;
        public Tile selectedTile = null;

        public static GridBuilder instanceGridBuilder = null;

        //grid size in hexes
        public int gridWidthInHexes = 4;
        public int gridHeightInHexes = 5;
    

        List<GameObject> m_path;
        public Dictionary<Point, TileBehaviour> board;

        public TileBehaviour originTileTb = null;
        public TileBehaviour destinationTileTb = null;
    
        //hex size
        float m_hexWidth = 1f;
        float m_hexHeight = 1f;
    
        private void Awake()
        {
            Debug.Log("GridBuilder ...");
            instanceGridBuilder = this;
            //SetHexSize();
            CreateGrid();
            GenerateAndShowPath();
        }

        //set hex size depends on HEX-prefabs (need renderer component)
        void SetHexSize(float _Width, float _Height)
        {
            this.m_hexWidth =_Width;
            this.m_hexHeight = _Height;
        }

        //calc the position of first hex
        Vector3 CalulateInitPos()
        {
            Vector3 initPos = new Vector3(-this.m_hexWidth * this.gridWidthInHexes / 2f + this.m_hexWidth / 2, 0,
                this.gridHeightInHexes / 2f * this.m_hexHeight - this.m_hexHeight / 2);
            return initPos;
        }

        //convert hex grid coords to world coords
        public Vector3 CalcWorldCoord(Vector2 _GridPos)
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

        public Vector2 CalcGridPos(Vector3 _Coord)
        {
            _Coord.z -= -0.05f;
            Vector3 initPos = CalulateInitPos();
            Vector2 gridPos = new Vector2();
            float offset = 0;
            gridPos.y = Mathf.RoundToInt((initPos.z - _Coord.y) / (m_hexHeight * 1.75f));
            if (gridPos.y % 2 != 0)
                offset = m_hexWidth / 2;
            gridPos.x = Mathf.RoundToInt((_Coord.x - initPos.x - offset) / m_hexWidth);
            return gridPos;
        }

        //creation and positioning
        void CreateGrid()
        {
            Vector2 gridSize = new Vector2(gridWidthInHexes, gridHeightInHexes);
            GameObject hexGridGameObject = new GameObject("HexGrid");
            board = new Dictionary<Point, TileBehaviour>();
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
                    board.Add(tileBehaviour.tile.Location, tileBehaviour);
                }
            }
            int counter = 0;
            //Neighboring tile coordinates of all the tiles are calculated
            try
            {
                foreach (TileBehaviour tb in board.Values)
                {
                    tb.tile.FindNeighbours(board, new Vector2(gridWidthInHexes, gridHeightInHexes), true);
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

        private void DrawPath(IEnumerable<Tile> _Path)
        {
            if (this.m_path == null)
                this.m_path = new List<GameObject>();
            //Destroy game objects which used to indicate the path
            this.m_path.ForEach(Destroy);
            this.m_path.Clear();

            //Lines game object is used to hold all the "Line" game objects indicating the path
            GameObject lines = GameObject.Find("Lines");
            if (lines == null)
                lines = new GameObject("Lines");
            foreach (Tile tile in _Path)
            {
                var line = (GameObject)Instantiate(this.line);
                //calcWorldCoord method uses squiggly axis coordinates so we add y / 2 to convert x coordinate from straight axis coordinate system
                Vector2 gridPos = new Vector2(tile.X, tile.Y);
                line.transform.position = CalcWorldCoord(gridPos);
                this.m_path.Add(line);
                line.transform.parent = lines.transform;
            }
        }

        public void GenerateAndShowPath()
        {
            //Don't do anything if origin or destination is not defined yet
            if (originTileTb == null || this.destinationTileTb == null)
            {
                DrawPath(new List<Tile>());
                return;
            }
            //We assume that the distance between any two adjacent tiles is 1
            //If you want to have some mountains, rivers, dirt roads or something else which might slow down the player you should replace the function with something that suits better your needs
            var path = PathFinderInGame.FindPath(originTileTb.tile, this.destinationTileTb.tile);
            DrawPath(path);
            MovementController mc = CombatController.instanceCombatController.selectedUnit.GetComponent<MovementController>();
            mc.StartMoving(path.ToList());
        }
    }
}

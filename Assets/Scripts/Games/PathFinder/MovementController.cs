using System.Collections.Generic;
using UnityEngine;

namespace Games.PathFinder
{
    public class MovementController : MonoBehaviour
    {
        //speed in meters per second
        public float speed = 0.0025F;
        public float rotationSpeed = 0.004F;
        //distance between character and tile position when we assume we reached it and start looking for the next. Explained in detail later on
        public static float MinNextTileDist = 0.05f;

        private CharacterController m_controller;
        public static MovementController instanceMovementController = null;
        //position of the tile we are heading to
        Vector3 m_curTilePos;
        Tile m_curTile;
        int m_curIndex;
        List<Tile> m_path;
        public bool IsMoving { get; private set; }
        Transform m_myTransform;

        void Awake()
        {        
            instanceMovementController = this;
            IsMoving = false;
        }

        void Start()
        {
            m_controller = this.GetComponent<CharacterController>();
            //caching the transform for better performance
            m_myTransform = transform;
        }       

        //gets tile position in world space
        Vector3 CalcTilePos(Tile _Tile)
        {
            Vector2 tileGridPos = new Vector2(_Tile.X, _Tile.Y);
            Vector3 tilePos = GridBuilder.instanceGridBuilder.CalcWorldCoord(tileGridPos);
            //y coordinate is disregarded
            tilePos.z = m_myTransform.position.z;
            return tilePos;
        }

        //method argument is a list of tiles we got from the path finding algorithm
        public void StartMoving(List<Tile> _Path)
        {
            if (_Path.Count == 0)
                return;
            //the first tile we need to reach is actually in the end of the list just before the one the character is currently on
            m_curIndex = _Path.Count - 2;
            m_curTile = _Path[m_curIndex];
            m_curTilePos = CalcTilePos(m_curTile);
            IsMoving = true;
            this.m_path = _Path;
        }

        //Method used to switch destination and origin tiles after the destination is reached
        void switchOriginAndDestinationTiles()
        {
            GridBuilder GM = GridBuilder.instanceGridBuilder;
            Material originMaterial = GM.originTileTb.GetComponent<Renderer>().material;
            //GM.originTileTb.GetComponent<Renderer>().material = GM.destinationTileTb.defaultMaterial;
            GM.originTileTb = GM.destinationTileTb;
            GM.originTileTb.GetComponent<Renderer>().material = originMaterial;
            GM.destinationTileTb = null;
            // GM.GenerateAndShowPath();
        }

        void Update()
        {
            if (!IsMoving)
                return;
            if ((m_curTilePos - m_myTransform.position).sqrMagnitude < MinNextTileDist * MinNextTileDist)
            {
                //if we reached the destination tile
                if (m_curIndex == 0)
                {
                    IsMoving = false;
                    m_curTile.isPassable = !m_curTile.isPassable;
                    switchOriginAndDestinationTiles();
                    return;
                }
                //curTile becomes the next one
                m_curIndex--;
                m_curTile = m_path[m_curIndex];
                m_curTilePos = CalcTilePos(m_curTile);
            }

            MoveTowards(m_curTilePos);
        }

        void MoveTowards(Vector3 _Position)
        {
            //mevement direction
            Vector3 dir = _Position - m_myTransform.position;

            // Rotate towards the target
            m_myTransform.rotation = Quaternion.Slerp(m_myTransform.rotation, Quaternion.LookRotation(dir, Vector3.back), rotationSpeed * Time.deltaTime);

            Vector3 forwardDir = m_myTransform.forward;
            forwardDir = forwardDir * speed;
            float speedModifier = Vector3.Dot(dir.normalized, m_myTransform.forward);
            forwardDir *= speedModifier;
            m_controller.Move(forwardDir);
        }
    }
}


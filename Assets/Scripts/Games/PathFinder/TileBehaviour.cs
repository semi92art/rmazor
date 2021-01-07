using System;
using Shapes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Games.PathFinder
{
    public class TileBehaviour : MonoBehaviour
    {
        public Tile tile;

       void OnMouseEnter()
        {
            PathFinderManager.Instance.selectedTile = tile;
            Debug.Log("This:" + tile);
        }

        private void OnMouseUp()
        {
            Debug.Log("OnMouseUp!");
        }

        //called every frame when mouse cursor is on this tile
        void OnMouseOver()
        {
            // //if player right-clicks on the tile, toggle passable variable and change the color accordingly
            if (Input.GetMouseButtonUp(1))
            {
                Debug.Log("RM!");
            if (this == PathFinderManager.Instance.destinationTileTb ||
                this == PathFinderManager.Instance.originTileTb)
                return;
            tile.isPassable = !tile.isPassable;
            if (!tile.isPassable)
                ChangeColor(Color.gray);
            else
                ChangeColor(Color.red);
            
           // GridBuilder.instanceGridBuilder.GenerateAndShowPath();
            }
            // //if user left-clicks the tile
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("LM!");
            tile.isPassable = true;
            
            TileBehaviour originTileTb = PathFinderManager.Instance.originTileTb;
            //if user clicks on origin tile or origin tile is not assigned yet
            if (this == originTileTb || originTileTb == null)
                //CombatController.instanceCombatController.selectedUnit =
                OriginTileChanged();
            else
                DestTileChanged();
            
            //GridBuilder.instanceGridBuilder.GenerateAndShowPath();
            }
        }

        void OriginTileChanged()
        {
            var originTileTb = PathFinderManager.Instance.originTileTb;
            //deselect origin tile if user clicks on current origin tile
            if (this == originTileTb)
            {
                PathFinderManager.Instance.originTileTb = null;
                return;
            }
            //if origin tile is not specified already mark this tile as origin
            PathFinderManager.Instance.originTileTb = this;
            ChangeColor(Color.red);
            Debug.Log("Start:" + this.tile);
        }

        void DestTileChanged()
        {
            var destTile = PathFinderManager.Instance.destinationTileTb;
            //deselect destination tile if user clicks on current destination tile
            if (this == destTile)
            {
                PathFinderManager.Instance.destinationTileTb = null;
                return;
            }
            //if there was other tile marked as destination, change its material to default (fully transparent) one
            if (destTile != null)
                PathFinderManager.Instance.destinationTileTb = this;
            ChangeColor(Color.blue);
            Debug.Log("End:" + this.tile);
        }
        
        void ChangeColor(Color _Color)
        {
            GetComponent<RegularPolygon>().Color = _Color;
        }

        public void SetAsOrigin()
        {
            ChangeColor(Color.red);
            tile.isPassable = false;
            GridBuilder.instanceGridBuilder.originTileTb = this;
        }

        public void SetPassable()
        {
            tile.isPassable = true;
        }

        public void SetImPassable()
        {
            ChangeColor(Color.gray);
            tile.isPassable = false;
        }

        public Tile GetTile()
        {
            return tile;
        }
    }
}

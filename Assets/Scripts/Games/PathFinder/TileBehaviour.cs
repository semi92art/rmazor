using UnityEngine;

namespace Games.PathFinder
{
    public class TileBehaviour : MonoBehaviour
    {
        public Tile tile;
        //After attaching this script to hex tile prefab don't forget to initialize following materials with the ones we created earlier
        public Material OpaqueMaterial;
        public Material defaultMaterial;
        //Slightly transparent orange
        Color orange = new Color(255f / 255f, 127f / 255f, 0, 127f / 255f);


        void changeColor(Color color)
        {
            //If transparency is not set already, set it to default value
            if (color.a == 1)
                color.a = 130f / 255f;
            GetComponent<Renderer>().material = OpaqueMaterial;
            GetComponent<Renderer>().material.SetColor("_BaseColor", color);
        }

        //IMPORTANT: for methods like OnMouseEnter, OnMouseExit and so on to work, collider (Component -> Physics -> Mesh Collider) should be attached to the prefab
        void OnMouseEnter()
        {
            GridBuilder.instanceGridBuilder.selectedTile = tile;
            //when mouse is over some tile, the tile is passable and the current tile is neither destination nor origin tile, change color to orange
            if (tile.isPassable && this != GridBuilder.instanceGridBuilder.destinationTileTb
                                && this != GridBuilder.instanceGridBuilder.originTileTb)
            {
                changeColor(new Color(7f / 255f, 185f / 255f, 0, 255f / 255f));
            }
            Debug.Log("This:" + tile);
        }

        //changes back to fully transparent material when mouse cursor is no longer hovering over the tile
        void OnMouseExit()
        {
            GridBuilder.instanceGridBuilder.selectedTile = null;
            if (tile.isPassable && this != GridBuilder.instanceGridBuilder.destinationTileTb
                                && this != GridBuilder.instanceGridBuilder.originTileTb)
            {
                this.GetComponent<Renderer>().material = defaultMaterial;
            }
        }
        //called every frame when mouse cursor is on this tile
        void OnMouseOver()
        {
            //if player right-clicks on the tile, toggle passable variable and change the color accordingly
            if (Input.GetMouseButtonUp(1))
            {
                if (this == GridBuilder.instanceGridBuilder.destinationTileTb ||
                    this == GridBuilder.instanceGridBuilder.originTileTb)
                    return;
                tile.isPassable = !tile.isPassable;
                if (!tile.isPassable)
                    changeColor(Color.gray);
                else
                    changeColor(Color.red);

                GridBuilder.instanceGridBuilder.GenerateAndShowPath();
            }
            //if user left-clicks the tile
            if (Input.GetMouseButtonUp(0))
            {
                tile.isPassable = true;

                TileBehaviour originTileTB = GridBuilder.instanceGridBuilder.originTileTb;
                //if user clicks on origin tile or origin tile is not assigned yet
                if (this == originTileTB || originTileTB == null)
                    //CombatController.instanceCombatController.selectedUnit =
                    originTileChanged();
                else
                    destTileChanged();

                GridBuilder.instanceGridBuilder.GenerateAndShowPath();
            }
        }

        void originTileChanged()
        {
            var originTileTB = GridBuilder.instanceGridBuilder.originTileTb;
            //deselect origin tile if user clicks on current origin tile
            if (this == originTileTB)
            {
                GridBuilder.instanceGridBuilder.originTileTb = null;
                GetComponent<Renderer>().material = defaultMaterial;
                return;
            }
            //if origin tile is not specified already mark this tile as origin
            GridBuilder.instanceGridBuilder.originTileTb = this;
            changeColor(Color.red);
            Debug.Log("Start:" + this.tile);
        }

        void destTileChanged()
        {
            var destTile = GridBuilder.instanceGridBuilder.destinationTileTb;
            //deselect destination tile if user clicks on current destination tile
            if (this == destTile)
            {
                GridBuilder.instanceGridBuilder.destinationTileTb = null;
                GetComponent<Renderer>().material.color = orange;
                return;
            }
            //if there was other tile marked as destination, change its material to default (fully transparent) one
            if (destTile != null)
                destTile.GetComponent<Renderer>().material = defaultMaterial;
            GridBuilder.instanceGridBuilder.destinationTileTb = this;
            changeColor(Color.blue);
            Debug.Log("End:" + this.tile);
        }

        public void setAsOrigin()
        {
            changeColor(Color.red);
            tile.isPassable = false;
            GridBuilder.instanceGridBuilder.originTileTb = this;
        }

        public void setPassable()
        {
            GetComponent<Renderer>().material = defaultMaterial;
            tile.isPassable = true;
        }

        public void setImPassable()
        {
            changeColor(Color.gray);
            tile.isPassable = false;
        }

        public Tile GetTile()
        {
            return tile;
        }
    }
}

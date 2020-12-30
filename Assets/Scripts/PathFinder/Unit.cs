using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Start is called before the first frame update
    public int health;
    public int amount;
    public Vector2 positionTile;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        positionTile = GridBuilder.instanceGridBuilder.CalcGridPos(transform.position);
        Point point = new Point((int)(positionTile.x),(int)positionTile.y);
        TileBehaviour tb = GridBuilder.instanceGridBuilder.board[point];
        //tb.setImPassable();
    }
}

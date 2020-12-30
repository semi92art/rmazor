using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public GameObject selectedUnit;
    public static CombatController instanceCombatController = null;
    // Start is called before the first frame update
    void Awake()
    {
        instanceCombatController = this;
        
    }

    private void Start()
    {
        selectedUnit = GetComponent<GamePlayController>().PlayerUnits[0];
    }

    void selectUnit()
    {
        // GridBuilder.instanceGridBuilder.originTileTb.setPassable();
        // Vector2 positionTile = GridBuilder.instanceGridBuilder.CalcGridPos(selectedUnit.transform.position);
        // Point point = new Point((int)(positionTile.x), (int)positionTile.y);
        // TileBehaviour tb = GridBuilder.instanceGridBuilder.board[point];
        // tb.setAsOrigin();
    }
}

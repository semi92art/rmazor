using System.Collections.Generic;
using UnityEngine;

namespace Games.PathFinder
{
    public class GamePlayController : MonoBehaviour
    {
        public GameObject StarshipL;

        public List<GameObject> PlayerUnits;

        private Vector3 initPosition = new Vector3(0, 0, -0.1f);
        // Start is called before the first frame update
        private void Awake()
        {
            Debug.Log("UnitsInit ...");
            UnitsInit();
        }

        private void UnitsInit()
        {
            PlayerUnits = new List<GameObject>();
            GameObject sl = Instantiate(StarshipL);
            sl.transform.position = initPosition + GridBuilder.instanceGridBuilder.CalcWorldCoord(new Vector2(0, 0));
            sl.transform.rotation = Quaternion.Euler(new Vector3(0, 90, -90));
            sl.GetComponent<Unit>().positionTile = new Vector2(0, 0);
            PlayerUnits.Add(sl);

            Debug.Log("UnitsInit done");
        }
    }
}

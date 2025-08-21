using UnityEngine;
using LibraryCodes;

public class Testing : MonoBehaviour
{
    private Grid grid;
    private BattleSystem BattleSystem;

    private void Start()
    {
        grid = new Grid(44, 44, 1f, new Vector3(-18f, -10f));
    }

    private void Update()
    {

        //if (Input.GetMouseButtonDown(0))
        //{
        //    grid.SetValue(UtilityMovement.GetMouseWorldPosition(), 56);
        //}

        //if (Input.GetMouseButtonDown(1))
        //{
        //   Debug.Log(grid.GetValue(UtilityMovement.GetMouseWorldPosition()));
        //}
    }
}

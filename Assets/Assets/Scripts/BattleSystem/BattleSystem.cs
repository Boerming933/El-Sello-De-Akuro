using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public enum BattleState{ START, PLAYERTURN, ENEMYTURN, WON, LOST}

public class BattleSystem : MonoBehaviour
{
    public List<GameObject> PlayersPrefab;
    public List<GameObject> EnemiesPrefab;
    public BattleState state;

    public List<Transform> PlayerBattleStation;
    public List<Transform> EnemyBattleStation;

    public bool PlayerInTurn = true;
    int Turn = 1;

    Unit PlayerUnity;
    Unit EnemyUnity;

    void Start()
    {
        state = BattleState.START;

        Debug.Log(Turn);
        Debug.Log(PlayerInTurn);
        SetupBattle();
    }

    void SetupBattle()
    {
        int Enemies = EnemiesPrefab.Count;
        int Player = PlayersPrefab.Count;
        int SizeFor;

        if (Enemies >= Player)
        {
            SizeFor = Enemies;
        }
        else SizeFor = Player;


        //for (float x = SizeFor; x >= PlayersPrefab.Count && x >= EnemiesPrefab.Count; x++)
        //{
        //    if (PlayersPrefab.Count > 0)
        //    {
        //        Instantiate(PlayersPrefab[Player], PlayerBattleStation[Player]);
        //        Player--;
        //    }
        //    if (EnemiesPrefab.Count > 0)
        //    {
        //        Instantiate(EnemiesPrefab[Enemies], PlayerBattleStation[Enemies]);
        //        Enemies--;
        //    }
        //}
    }

    //void PlayerTurn()
    //{
    //    if (!PlayerInTurn)
    //    {
    //        return;
    //    }
    //    int Enemies = EnemiesPrefab.Count;
    //}

    //void EnemyTurn()
    //{
    //    if (PlayerInTurn)
    //    {
    //        return;
    //    }
    //    int Player = PlayersPrefab.Count;
    //}

    private void Update()
    {        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        Turn++;
        Debug.Log(Turn);
        Debug.Log(PlayerInTurn);

        if (!PlayerInTurn)
        {
            PlayerInTurn = true;
        }
        else if (PlayerInTurn)
        {
            PlayerInTurn = false;
        }
    }
}

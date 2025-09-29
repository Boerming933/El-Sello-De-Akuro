using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackControllerEnemy : MonoBehaviour
{
    [Header("Inventario de ataques")]
    public List<AttackData> allAttacks = new List<AttackData>();

    public bool inAttackMode = false;
    public bool playerHit = false;

    public Color impactColor = new Color(1f, 0f, 0f, 0.5f);

    OverlayTile Player1, Player2, Player3;

    public float[] Atkcooldowns;
    public float[] actualCD;
    public bool[] onCooldown;

    OverlayTile finalMove, attackOrigin;

    [SerializeField] private AttackData currentAttack;
    [SerializeField] private int currentAttackIndex = -1;

    [SerializeField] private RangeFinder rangeFinder;

    private Unit currentUnit;
    public EnemyIA enemyIA;

    private List<BattleHUD> hudsToReset = new();

    public OverlayTile AttackTile() => attackOrigin;  // select/center del ataque
    public OverlayTile FinalMoveTile() => finalMove;     // casilla a la que conviene moverse
    public AttackData ChosenAttack() => currentAttack;

    private void Awake()
    {
        if (rangeFinder == null) rangeFinder = new RangeFinder();

        int attackCount = allAttacks.Count;
        Atkcooldowns = new float[attackCount];
        actualCD = new float[attackCount];
        onCooldown = new bool[attackCount];

        for (int i = 0; i < attackCount; i++)
        {
            Debug.LogError("El i del ataque numero es " + i);
            Atkcooldowns[i] = allAttacks[i].cooldown;
            actualCD[i] = 0f;
            onCooldown[i] = false;
        }
    }

    public void playerPosition(OverlayTile player1, OverlayTile player2, OverlayTile player3)
    {
        Player1 = player1;
        Player2 = player2;
        Player3 = player3;
    }

    /*  <summary>
        se puede golpear a alguno de los playerTiles.
        </summary>*/
    public bool CanAttackFrom(List<OverlayTile> MovementTiles, OverlayTile Active)
    {
        currentAttack = null; attackOrigin = null; finalMove = null;
        currentAttackIndex = -1;

        if (Player1 == null || Player2 == null || Player3 == null) { Debug.LogError("El problema esta en Players"); return false; }
        if (rangeFinder == null) { Debug.LogError("El problema esta en rangeFinder"); return false; }
        if (MovementTiles == null || MovementTiles.Count == 0) { Debug.LogError("El problema esta en moveTiles"); return false; }
        if (allAttacks == null || allAttacks.Count == 0) { Debug.LogError("El problema esta en allAttacks"); return false; }

        if (Active != null && !MovementTiles.Contains(Active))
            MovementTiles.Insert(0, Active);

        foreach (var move in MovementTiles)
        {
            for (int i = 0; i < allAttacks.Count; i++)
            {
                var atk = allAttacks[i];
                if (onCooldown[i]) continue;
                // 1) Tiles desde donde seleccionar 
                var selectTiles = rangeFinder.GetTilesInRange(move, atk.selectionRange) ?? new List<OverlayTile>();
                foreach (var tile in selectTiles)
                {
                    var area = GetEffectArea(tile, atk) ?? new List<OverlayTile>();
                    if (area.Count == 0) continue;

                    // 3) Comprueba si alguno de los playerTiles está en el área 

                    if (area.Contains(Player1) || area.Contains(Player2) || area.Contains(Player3))
                    {
                        if (finalMove == Active) return false;
                        currentAttack = atk;
                        currentAttackIndex = i;
                        finalMove = move;
                        attackOrigin = tile;
                    }
                }
            }
        }

        Player1 = null; Player2 = null; Player3 = null;

        if (finalMove != Active && finalMove != null) return true;
        else return false;
    }

    public List<OverlayTile> GetEffectArea(OverlayTile center, AttackData attack)
    {
        if (center == null || attack == null) return new List<OverlayTile>();

        var mapTiles = MapManager.Instance.map.Values;
        var area = new List<OverlayTile>();
        Vector2 origin = center.transform.position;
        float threshold = 0.1f;
        int size = attack.areaSize;

        switch (attack.effectShape)
        {
            case AreaShape.Circle:
                area = rangeFinder.GetTilesInRange(center, size);
                break;
            case AreaShape.LineHorizontal:
                Vector2 isoH = new Vector2(0.5f, 0.25f);
                for (int i = -size; i <= size; i++)
                {
                    Vector2 samplePos = origin + isoH * i;
                    var tile = mapTiles.FirstOrDefault(t => Vector2.Distance(t.transform.position, samplePos) < threshold);
                    if (tile != null) area.Add(tile);
                }
                break;
            case AreaShape.LineVertical:
                Vector2 isoV = new Vector2(0.5f, -0.25f);
                for (int i = -size; i <= size; i++)
                {
                    Vector2 samplePos = origin + isoV * i;
                    var tile = mapTiles.FirstOrDefault(t => Vector2.Distance(t.transform.position, samplePos) < threshold);
                    if (tile != null) area.Add(tile);
                }
                break;
            case AreaShape.Cross:
                area.Add(center);
                Vector2 pureX = new Vector2(1f, 0f);
                Vector2 pureY = new Vector2(0f, 0.5f);
                for (int d = 1; d <= size; d++)
                {
                    var positions = new[]
                    {
                        origin - pureX * d,
                        origin + pureX * d,
                        origin + pureY * d,
                        origin - pureY * d
                    };
                    foreach (var pos in positions)
                    {
                        var tile = mapTiles.FirstOrDefault(t => Vector2.Distance(t.transform.position, pos) < threshold);
                        if (tile != null && !area.Contains(tile))
                            area.Add(tile);
                    }
                }
                break;
        }

        return area;
    }

    public void ConfirmAttack(OverlayTile targetTile, AttackData attack)
    {
        if (targetTile == null || attack == null)
        {
            Debug.LogError("attack es null by Gabite");
            return;
        }

        if (currentAttackIndex >= 0 && currentAttackIndex < onCooldown.Length)
        {
            onCooldown[currentAttackIndex] = true;
            actualCD[currentAttackIndex] = Atkcooldowns[currentAttackIndex];
            Debug.Log($"Attack {currentAttack.name} put on cooldown for {actualCD[currentAttackIndex]} turns");
        }

        var area = GetEffectArea(targetTile, currentAttack);

        hudsToReset.Clear();

        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
            foreach (var col in hits)
            {
                if (col.CompareTag("Player"))
                {
                    var u = col.GetComponent<Unit>();
                    var playerHUDObj = col.transform.Find("HUDSecundaria/PlayerHUD");
                    if (playerHUDObj != null)
                    {
                        var PlayerHUD = playerHUDObj.GetComponent<BattleHUD>();
                        if (PlayerHUD != null && !hudsToReset.Contains(PlayerHUD))
                        {
                            PlayerHUD.SetHUD(u);
                            PlayerHUD.ApplyDamage(Mathf.RoundToInt(currentAttack.damage + currentUnit.Fue * currentAttack.scalingFactor));                                 //////
                            PlayerHUD.Show();
                            hudsToReset.Add(PlayerHUD);
                        }
                    }
                }
            }
        }

        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
            foreach (var col in hits)
            {
                if (col.TryGetComponent<Unit>(out Unit u) && u.CompareTag("Player"))
                    u.TakeDamage(Mathf.RoundToInt(currentAttack.damage + currentUnit.Fue * currentAttack.scalingFactor));                                                    /////
            }
        }

        StartCoroutine(ShowImpactAndFinish(area));
    }

    IEnumerator ShowImpactAndFinish(List<OverlayTile> area)
    {
        area.ForEach(t => t.ShowOverlay(impactColor));

        if (currentAttack.initiativeBonus != 0)
        {
            var im = FindAnyObjectByType<InitiativeManager>();
            im.ApplyInitiativeBuff(
                currentUnit,
                currentAttack.initiativeBonus,
                currentAttack.initiativeDuration
            );
        }

        yield return new WaitForSeconds(3f);
        MapManager.Instance.HideAllTiles();
        enemyIA.FinishTurn();

        foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            var hitMarker = playerObj.transform.Find("HitMarker");
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);
        }
    }

    public void ReduceCooldowns()
    {
        for (int i = 0; i < allAttacks.Count; i++)
        {
            if (actualCD[i] > 0 && onCooldown[i])
            {
                actualCD[i]--;
                Debug.Log($"Attack {allAttacks[i].name} cooldown reduced to {actualCD[i]}");
                if (actualCD[i] <= 0)
                {
                    onCooldown[i] = false;
                    Debug.Log($"Attack {allAttacks[i].name} is now off cooldown");
                }
            }
        }
    }    
}
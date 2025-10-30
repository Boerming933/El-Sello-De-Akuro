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

    public OverlayTile Player1, Player2, Player3;

    public float[] Atkcooldowns;
    public float[] actualCD;
    public bool[] onCooldown;

    public OverlayTile finalMove, attackOrigin;

    [SerializeField] private AttackData currentAttack;
    [SerializeField] private int currentAttackIndex = -1;

    [SerializeField] private RangeFinder rangeFinder;

    public Unit currentUnit;
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


       var players = new List<OverlayTile>{ Player1, Player2, Player3 };

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null)
            {
                players.RemoveAt(i);
                break;
            }
        }

        if (rangeFinder == null) { Debug.LogError("El problema esta en rangeFinder"); return false; }
        if (Active == null) { Debug.LogError("El problema esta en Active"); return false; }
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
                var attackerTile = currentUnit?.FindCenterTile();
                if (attackerTile != null)
                    selectTiles.RemoveAll(t => t.grid2DLocation == attackerTile.grid2DLocation); // ← ADD //
                foreach (var tile in selectTiles)
                {
                    var area = GetEffectArea(tile, atk) ?? new List<OverlayTile>();
                    if (area.Count == 0) continue;

                    // 3) Comprueba si alguno de los playerTiles está en el área 
                                        if (players.Any(area.Contains))
                    {
                        if (finalMove == Active)
                        {
                            return false;
                        }
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
        else
        {
            return false;
        }
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
            Debug.LogError($"Attack {currentAttack.name} put on cooldown for {actualCD[currentAttackIndex]} turns");
        }
        var unitDamageMap = new Dictionary<Unit, int>();
        var area = GetEffectArea(targetTile, attack);
        hudsToReset.Clear();

        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.2f);
            foreach (var col in hits)
            {
                if (col.isTrigger) continue; //
                if (col.CompareTag("Aliado") || col.CompareTag("Aliado2"))
                {
                    var u = col.GetComponent<Unit>();
                    if (!u.gameObject.activeInHierarchy) continue;
                    Debug.Log($"Applying attack effects to {u.name}");
                    int actualDamage = ApplyAttackToUnit(u);
                    if (actualDamage > 0)
                        unitDamageMap[u] = actualDamage;

                    var playerHUDObj = col.transform.Find("HUDSecundaria/PlayerHUD");
                    if (playerHUDObj != null)
                    {
                        var playerHUD = playerHUDObj.GetComponent<BattleHUD>();
                        if (playerHUD != null && !hudsToReset.Contains(playerHUD))
                        {
                            playerHUD.SetHUD(u);
                            if (unitDamageMap.ContainsKey(u) && unitDamageMap[u] > 0)
                            {
                                //gabiteHUD.ApplyDamage(unitDamageMap[u]);
                                Debug.LogError($"HUD showing real damage: {unitDamageMap[u]} to {u.name}");
                            }                                //////
                            playerHUD.Show();
                            hudsToReset.Add(playerHUD);
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
                if (col.isTrigger) continue; //
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

        yield return new WaitForSeconds(2f);
        MapManager.Instance.HideAllTiles();
        if (enemyIA != null && enemyIA.currentUnit == currentUnit)
        {
            enemyIA.FinishTurn();
        }

        foreach (var playerObj in GameObject.FindGameObjectsWithTag("Player"))
        {
            var hitMarker = playerObj.transform.Find("HitMarker");
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);
        }

        foreach (var hud in hudsToReset)
        {
            if(hud != null) hud?.Hide();

        }
        hudsToReset.Clear();

        yield break;
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
    
    int ApplyAttackToUnit(Unit targetUnit)
    {
        // ✅ FIXED: Safe property access for criticals
        float criticalChance = 0f;
        float criticalMultiplier = 1f;
        int finalDamage = 0;

        var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
        if (buffDebuffAttack != null)
        {
            criticalChance = buffDebuffAttack.criticalChance;
            criticalMultiplier = buffDebuffAttack.criticalMultiplier;
        }

        bool isCritical = Random.value < criticalChance;
        float damageMultiplier = isCritical ? criticalMultiplier : 1.0f;

        if (currentAttack.damage > 0)
        {
            int baseAttackDamage = currentAttack.damage + Mathf.RoundToInt(currentUnit.Fue * currentAttack.scalingFactor);

            var attackerStatusManager = currentUnit.GetComponent<StatusEffectManager>();
            if (attackerStatusManager != null)
            {
                float bonusPercent = attackerStatusManager.CalculateAttackBonusPercent();
                if (bonusPercent != 0f)
                {
                    baseAttackDamage = Mathf.RoundToInt(baseAttackDamage * (1f + bonusPercent));
                    baseAttackDamage = Mathf.Max(0, baseAttackDamage);
                    Debug.Log($"{currentUnit.name} attack bonus: +{bonusPercent * 100}% (base damage with bonus: {baseAttackDamage})");
                }
            }

            finalDamage = Mathf.RoundToInt(baseAttackDamage * damageMultiplier);
            finalDamage = Mathf.Max(0, finalDamage);

            var statusManager = targetUnit.GetComponent<StatusEffectManager>();
            if (statusManager != null)
            {
                float damageReduction = statusManager.CalculateDamageReduction();
                finalDamage = Mathf.RoundToInt(finalDamage * (1f - damageReduction));
                finalDamage = Mathf.Max(0, finalDamage);

                statusManager.SetLastAttacker(currentUnit);

                if (statusManager.HasEffect(StatusEffectType.DraconicStance))
                {
                    Debug.Log($"{targetUnit.name} negates all damage with Draconic Stance!");
                    statusManager.TriggerEffect(statusManager.GetEffect(StatusEffectType.DraconicStance), EffectTrigger.OnDamageReceived);
                    finalDamage = 0;
                }
                else if (statusManager.HasEffect(StatusEffectType.Guard))
                {
                    statusManager.TriggerEffect(statusManager.GetEffect(StatusEffectType.Guard), EffectTrigger.OnDamageReceived);
                }
            }

            if (finalDamage > 0)
            {
                targetUnit.TakeDamage(finalDamage);
                if (isCritical) Debug.Log($"Critical hit! {finalDamage} damage to {targetUnit.name}");
            }
        }

        // Apply status effects (only if this is a BuffDebuffAttackData)
        if (buffDebuffAttack != null)
        {
            Debug.Log($"Applying {buffDebuffAttack.statusEffects.Count} status effects to {targetUnit.name}");
            foreach (var attackEffect in buffDebuffAttack.statusEffects)
            {
                Debug.Log($"Processing status effect: {attackEffect.statusEffect.effectName} with probability {attackEffect.probability}%");
                if (Random.value <= attackEffect.probability)
                {
                    var statusManager = targetUnit.GetComponent<StatusEffectManager>();
                    if (statusManager == null)
                    {
                        Debug.Log($"Adding StatusEffectManager to {targetUnit.name}");
                        statusManager = targetUnit.gameObject.AddComponent<StatusEffectManager>();
                    }

                    var effectToApply = attackEffect.statusEffect.Clone();
                    effectToApply.caster = currentUnit;
                    statusManager.ApplyEffect(effectToApply);
                    Debug.Log($"Successfully applied {effectToApply.effectName} to {targetUnit.name}");
                }
                else
                {
                    Debug.Log($"Status effect {attackEffect.statusEffect.effectName} failed probability check");
                }
            }

            // Push mechanics (only available for BuffDebuffAttackData)
            if (buffDebuffAttack.canPushTarget && Random.value < buffDebuffAttack.pushChance)
            {
                PushUnit(targetUnit, buffDebuffAttack.pushDistance);
            }
        }
        // Return the final damage dealt for HUD updates
        if (currentAttack.damage > 0)
        {
            return finalDamage;
        }
        return 0;
    }

    void PushUnit(Unit targetUnit, int distance)
    {
        var currentTile = targetUnit.FindCenterTile();
        if (currentTile == null) return;

        Vector2Int pushDirection = GetPushDirection(currentUnit.FindCenterTile(), currentTile);
        Vector2Int newPosition = currentTile.grid2DLocation + pushDirection * distance;

        if (MapManager.Instance.map.TryGetValue(newPosition, out OverlayTile newTile) && !newTile.isBlocked)
        {
            targetUnit.transform.position = newTile.transform.position;
            currentTile.isBlocked = false;
            newTile.isBlocked = true;
            Debug.Log($"{targetUnit.name} pushed {distance} tiles!");
        }
    }

    Vector2Int GetPushDirection(OverlayTile from, OverlayTile to)
    {
        Vector2Int delta = to.grid2DLocation - from.grid2DLocation;
        return new Vector2Int(Mathf.Clamp(delta.x, -1, 1), Mathf.Clamp(delta.y, -1, 1));
    }
}
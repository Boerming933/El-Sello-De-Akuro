using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffDebuffAttackController : MonoBehaviour
{
    [Header("Referencias")]
    public AttackSelectionUI attackUI;
    public MouseControler mouseController;
    public BattleSystem battleSystem;

    [Header("Components")]
    public AttackBools attackBools;

    [Header("Overlay Colors")]
    public Color rangeColor = new Color(1f, 1f, 0f, 0.5f);
    public Color previewColor = new Color(0f, 1f, 1f, 0.5f);
    public Color impactColor = new Color(1f, 0f, 0f, 0.5f);

    private bool inAttackMode = false;
    private Unit currentUnit;
    private AttackData currentAttack; // ✅ Changed to AttackData to support both types
    private List<OverlayTile> validTiles = new();
    private List<OverlayTile> previewTiles = new();
    private OverlayTile lastHoverTile;
    private RangeFinder rangeFinder = new RangeFinder();
    private bool attackExecuted;
    private List<BattleHUD> hudsToReset = new();

    // Facing system
    public enum Facing4
    {
        ArribaIzq = 0, // "y"
        ArribaDer = 1, // "x"
        AbajoDer = 2, // "-y"
        AbajoIzq = 3  // "-x"
    }

    void OnEnable() => attackUI.OnAttackChosen += EnterAttackMode;
    void OnDisable() => attackUI.OnAttackChosen -= EnterAttackMode;

    public void SetCurrentUnit(Unit u) => currentUnit = u;

    public void StartAttack(AttackData atk)
    {
        EnterAttackMode(atk);
    }

    void EnterAttackMode(AttackData atkData)
    {
        


        if (attackExecuted) return;
        attackExecuted = false;

        MapManager.Instance.HideAllTiles();
        validTiles.Clear();
        ClearPreview();

        currentAttack = atkData;
        
        // if (currentUnit == null) 
        // {
        //     Debug.LogError("No current unit set for attack!");
        //     return;
        // }

        if (mouseController != null) mouseController.enabled = false;
        
        float threshold = 0.1f;
        var centerTile = MapManager.Instance.map.Values.FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < threshold);          ////Intentar optimizar esto

        if (centerTile == null)
        {
            Debug.LogError($"No se encontró OverlayTile bajo {currentUnit.name}");
            return;
        }
        bool isSelfTargeting = ShouldAffectSelf();
        
        if (currentAttack.selectionRange == 0 || isSelfTargeting)
        {
            ConfirmAttack(centerTile);
            return;
        }
        
        validTiles = rangeFinder.GetTilesInRange(centerTile, currentAttack.selectionRange, currentAttack);
        
        var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
        bool allowSelfTargeting = false;
        
        if (buffDebuffAttack != null && currentAttack.selectionRange == 0)
        {
            bool hasSelfTargeting = buffDebuffAttack.statusEffects.Any(e => e.targetSelf);
            if (hasSelfTargeting && !validTiles.Contains(centerTile))
            {
                validTiles.Add(centerTile);
                allowSelfTargeting = true;
            }
        }
        
        if (!allowSelfTargeting && validTiles.Contains(centerTile))
        {
            validTiles.Remove(centerTile);
        }
        
        if (currentAttack.effectShape == AreaShape.Perpendicular)
        {
            Debug.Log($"[EnterAttackMode] Valid selection tiles: {string.Join(", ", validTiles.Select(t => $"({t.grid2DLocation.x},{t.grid2DLocation.y})"))}");
        }
        
        foreach (var tile in validTiles)
        {
            tile.ShowOverlay(rangeColor);
        }
        inAttackMode = true;
        lastHoverTile = null;
    }

    void Update()
    {
        if (!inAttackMode) return;
        UpdatePreviewUnderCursor();

        if (Input.GetMouseButtonDown(0))
        {
            var hovered = GetTileUnderCursor();
            if (hovered != null && validTiles.Contains(hovered))
                ConfirmAttack(hovered);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button1))
        {
            ExitAttackMode();
            attackBools.ResetDirectionStates();
        }
    }

    void UpdatePreviewUnderCursor()         ///////////////// optimizar los llamados con tags y getcomponent
    {
        var hovered = GetTileUnderCursor();
        if (hovered == null || !validTiles.Contains(hovered))
        {
            ClearPreview();
            lastHoverTile = null;
            return;
        }

        if (hovered != lastHoverTile)
        {
            // Determine facing for directional attacks
            float originThreshold = 0.1f;
            var playerTile = MapManager.Instance.map.Values
                .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < originThreshold);



            if (playerTile != null)
            {
                var facing = DetermineFacingFromTiles(playerTile, hovered);
                ApplyFacingToUnit(currentUnit, facing);



                // IMMEDIATELY after facing decision: update isLookingUp using same tiles
                if (attackBools != null)
                {
                    attackBools.CheckTileDirection(playerTile, hovered);

                    if (mouseController.myUnit.Name == "Riku Takeda")
                    {
                        if (mouseController.attackBools.isLookingUp == true)
                        {
                            mouseController.animatorSamurai.SetBool("idleBatallaUp", true);
                        }
                        else
                        {
                            mouseController.animatorSamurai.SetBool("idleBatallaUp", false);
                        }
                    }

                    if (mouseController.myUnit.Name == "Sayuri")
                    {
                        if (mouseController.attackBools.isLookingUp == true)
                        {
                            mouseController.animatorGeisha.SetBool("idleBatallaUp", true);
                        }
                        else
                        {
                            mouseController.animatorGeisha.SetBool("idleBatallaUp", false);
                        }
                    }

                    if (mouseController.myUnit.Name == "Raiden")
                    {
                        if (mouseController.attackBools.isLookingUp == true)
                        {
                            mouseController.animatorNinja.SetBool("idleBatallaUp", true);
                        }
                        else
                        {
                            mouseController.animatorNinja.SetBool("idleBatallaUp", false);
                        }
                    }
                }
            }

            ClearPreview();

            var area = GetEffectArea(hovered);
            previewTiles = area;
            previewTiles.ForEach(t => t.ShowOverlay(previewColor));
            lastHoverTile = hovered;

            // Enhanced enemy preview system
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemyObj in enemies)
            {
                var unit = enemyObj.GetComponent<Unit>();
                if (unit == null) continue;

                bool isInArea = previewTiles.Any(tile => Vector2.Distance(tile.transform.position, enemyObj.transform.position) < 0.2f);

                var gabiteHUDObj = enemyObj.transform.Find("HUDSecundaria/GabiteHUD");
                if (gabiteHUDObj != null)
                {
                    var gabiteHUD = gabiteHUDObj.GetComponent<BattleHUD>();
                    if (gabiteHUD != null)
                    {
                        if (isInArea)
                        {
                            gabiteHUD.SetHUD(unit);
                            int baseDamage = Mathf.RoundToInt(currentAttack.damage + currentUnit.Fue * currentAttack.scalingFactor);
                            var attackerStatusManager = currentUnit.GetComponent<StatusEffectManager>();
                            if (attackerStatusManager != null)
                            {
                                float bonusPercent = attackerStatusManager.CalculateAttackBonusPercent();
                                baseDamage = Mathf.RoundToInt(baseDamage * (1f + bonusPercent));
                                baseDamage = Mathf.Max(0, baseDamage);
                            }
                            gabiteHUD.PreviewDamage(baseDamage);
                            gabiteHUD.Show();
                            if (!hudsToReset.Contains(gabiteHUD)) hudsToReset.Add(gabiteHUD);
                        }
                        else
                        {
                            gabiteHUD.ResetPreview();
                            gabiteHUD.Hide();
                        }
                    }
                }

                var hitMarker = enemyObj.transform.Find("HitMarker");
                if (hitMarker != null)
                    hitMarker.gameObject.SetActive(isInArea);
            }
        }
    }

    void ConfirmAttack(OverlayTile targetTile)
    {
        if (mouseController.myUnit.Name == "Riku Takeda")
        {
            mouseController.animatorSamurai.SetBool("idleBatallaUp", false);
            mouseController.animatorSamurai.SetBool("idleBatalla", false);

            if (mouseController.attackBools.samuraiAttack1)              // ATAQUE 1
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorSamurai.SetTrigger("attack1Up");
                }
                else
                {
                    mouseController.animatorSamurai.SetTrigger("attack1");
                }

                AudioManager.Instance.PlaySFX("SamuraiAttack1");
                mouseController.attackBools.ResetAllSamuraiAttacks();
            }

            if (mouseController.attackBools.samuraiAttack2)               // ATAQUE 2
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorSamurai.SetTrigger("attack2Up");
                }
                else
                {
                    mouseController.animatorSamurai.SetTrigger("attack2");
                }

                AudioManager.Instance.PlaySFX("SamuraiAttack2");
                mouseController.attackBools.ResetAllSamuraiAttacks();
            }

            if (mouseController.attackBools.samuraiAttack3)               // ATAQUE 3
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorSamurai.SetTrigger("attack3Up");
                }
                else
                {
                    mouseController.animatorSamurai.SetTrigger("attack3");
                }

                AudioManager.Instance.PlaySFX("SamuraiAttack3");
                mouseController.attackBools.ResetAllSamuraiAttacks();
            }

            if (mouseController.attackBools.samuraiAttack4)               // ATAQUE 4
            {
                
                    mouseController.animatorSamurai.SetTrigger("attack4");
                

                AudioManager.Instance.PlaySFX("SamuraiAttack4");
                mouseController.attackBools.ResetAllSamuraiAttacks();
            }

            if (mouseController.attackBools.samuraiAttack5)               // ATAQUE 5
            {
                
                    mouseController.animatorSamurai.SetTrigger("attack4");
                mouseController.SamuraiShield.SetActive(true);       // convertir en false cuando de el contraataque


                AudioManager.Instance.PlaySFX("SamuraiAttack5");
                mouseController.attackBools.ResetAllSamuraiAttacks();
            }
        }

        if (mouseController.myUnit.Name == "Sayuri")
        {
            mouseController.animatorGeisha.SetBool("idleBatallaUp", false);
            mouseController.animatorGeisha.SetBool("idleBatalla", false);

            if (mouseController.attackBools.geishaAttack1)              // ATAQUE 1
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorGeisha.SetTrigger("attack1Up");
                    mouseController.geishaEffects.SetTrigger("attack1Up");
                }
                else
                {
                    mouseController.animatorGeisha.SetTrigger("attack1");
                    mouseController.geishaEffects.SetTrigger("attack1");

                }

                AudioManager.Instance.PlaySFX("GeishaAttack1");
                mouseController.attackBools.ResetAllGeishaAttacks();
            }

            if (mouseController.attackBools.geishaAttack3)              // ATAQUE 3
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorGeisha.SetTrigger("attack3Up");
                    mouseController.geishaEffects.SetTrigger("attack3Up");
                }
                else
                {
                    mouseController.animatorGeisha.SetTrigger("attack3");
                    mouseController.geishaEffects.SetTrigger("attack3");
                }

                AudioManager.Instance.PlaySFX("GeishaAttack3");
                mouseController.attackBools.ResetAllGeishaAttacks();
            }

            if (mouseController.attackBools.geishaAttack4)              // ATAQUE 4
            {
                mouseController.animatorGeisha.SetTrigger("attack4");
                AudioManager.Instance.PlaySFX("GeishaAttack4");

                mouseController.attackBools.ResetAllGeishaAttacks();
            }

            if (mouseController.attackBools.geishaAttack5)              // ATAQUE 5
            {
                mouseController.animatorGeisha.SetTrigger("attack5");
                AudioManager.Instance.PlaySFX("GeishaAttack5");

                mouseController.attackBools.ResetAllGeishaAttacks();
            }
        }



        if (mouseController.myUnit.Name == "Raiden")
        {
            mouseController.animatorNinja.SetBool("idleBatallaUp", false);
            mouseController.animatorNinja.SetBool("idleBatalla", false);

            if (mouseController.attackBools.ninjaAttack1)              // ATAQUE 1
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorNinja.SetTrigger("attack1Up");
                }
                else
                {
                    mouseController.animatorNinja.SetTrigger("attack1");
                }

                AudioManager.Instance.PlaySFX("NinjaAttack1");
                mouseController.attackBools.ResetAllNinjaAttacks();
            }

            if (mouseController.attackBools.ninjaAttack2)              // ATAQUE 2
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorNinja.SetTrigger("attack2Up");
                    mouseController.ninjaEffects.SetTrigger("attack2Up");
                }
                else
                {
                    mouseController.animatorNinja.SetTrigger("attack2");
                    mouseController.ninjaEffects.SetTrigger("attack2");
                }

                AudioManager.Instance.PlaySFX("NinjaAttack2");
                mouseController.attackBools.ResetAllNinjaAttacks();
            }

            if (mouseController.attackBools.ninjaAttack3)              // ATAQUE 3
            {
                if (mouseController.attackBools.isLookingUp == true)
                {
                    mouseController.animatorNinja.SetTrigger("attack3Up");
                    mouseController.ninjaEffects.SetTrigger("attack3Up");
                }
                else
                {
                    mouseController.animatorNinja.SetTrigger("attack3");
                    mouseController.ninjaEffects.SetTrigger("attack3");
                }

                AudioManager.Instance.PlaySFX("NinjaAttack3");
                mouseController.attackBools.ResetAllNinjaAttacks();
            }

            if (mouseController.attackBools.ninjaAttack4)              // ATAQUE 4
            {
               
                mouseController.animatorNinja.SetTrigger("attack4");
                    

                AudioManager.Instance.PlaySFX("NinjaAttack4");
                mouseController.attackBools.ResetAllNinjaAttacks();
            }
        }

        mouseController.canPocion = false;
        if (attackExecuted) return;

        if (currentAttack.manaCost > currentUnit.currentMana)
        {
            Debug.LogWarning($"[Safety Guard] {currentUnit.Name} attempted attack without enough mana! This shouldn't happen. Cost: {currentAttack.manaCost}, Current: {currentUnit.currentMana}");
            ExitAttackMode();
            attackBools.ResetDirectionStates();
            return;
        }

        currentUnit.currentMana -= currentAttack.manaCost;
        Debug.Log($"{currentUnit.Name} spent {currentAttack.manaCost} mana. Remaining: {currentUnit.currentMana}/{currentUnit.maxMana}");

        var detailsUI = UnityEngine.Object.FindFirstObjectByType<CharacterDetailsUI>();
        if (detailsUI != null)
            detailsUI.UpdateAllUI();

        if (currentUnit != null)
        {
            var statusManager = currentUnit.GetComponent<StatusEffectManager>();
            if (statusManager != null && statusManager.MustAttackNextTurn())
            {
                statusManager.ClearMustAttackCondition();
            }
        }

        var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
        if (buffDebuffAttack != null)
        {
            if (!buffDebuffAttack.CanUse()) return;
            buffDebuffAttack.UseAttack();
        }

        attackExecuted = true;
        attackUI.SetButtonsInteractable(false);

        validTiles.ForEach(t => t.HideTile());
        ClearPreview();
        inAttackMode = false;

        var panels = Object.FindObjectsByType<PanelAcciones>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in panels) p.Hide();

        var area = GetEffectArea(targetTile);
        hudsToReset.Clear();

        ExecuteAttackEffects(area);
        StartCoroutine(ShowImpactAndFinish(area));
    }


    void ExecuteAttackEffects(List<OverlayTile> area)                   ////////////////////////// OPTIMIZAR ESTO //////////////////////
    {
        var affectedUnits = new List<Unit>();

        // Collect all affected units
        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.3f);

            foreach (var col in hits)
            {
                var unit = col.GetComponent<Unit>();
                if (unit == null)
                {
                    Debug.Log($"Collider {col.name} has no Unit component");
                    continue;
                }

                bool isEnemy = col.CompareTag("Enemy");
                bool isAlly = col.CompareTag("Aliado") || col.CompareTag("Aliado2");
                bool isSelf = unit == currentUnit;

                //Debug.Log($"Found unit {unit.name}: isEnemy={isEnemy}, isAlly={isAlly}, isSelf={isSelf}");

                // For BuffDebuffAttackData, use targeting rules
                var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
                if (buffDebuffAttack != null)
                {
                    //Debug.Log($"Targeting rules: ShouldAffectEnemies={ShouldAffectEnemies()}, ShouldAffectAllies={ShouldAffectAllies()}, ShouldAffectSelf={ShouldAffectSelf()}");

                    if ((isEnemy && ShouldAffectEnemies()) || (isAlly && ShouldAffectAllies()) || (isSelf && ShouldAffectSelf()) || (isEnemy && currentAttack.damage > 0)) // Always damage enemies
                    {
                        if (!affectedUnits.Contains(unit))
                        {
                            affectedUnits.Add(unit);
                            //Debug.Log($"Added {unit.name} to affected units list");
                        }
                    }
                    else
                    {
                        Debug.Log($"Unit {unit.name} not affected by targeting rules");
                    }
                }
                else
                {
                    // For regular AttackData, target enemies for damage
                    if (isEnemy && !affectedUnits.Contains(unit))
                    {
                        affectedUnits.Add(unit);
                        //Debug.Log($"Added enemy {unit.name} to affected units list (regular attack)");
                    }
                }
            }
        }

        Debug.Log($"Total units to be affected: {affectedUnits.Count}");

        var unitDamageMap = new Dictionary<Unit, int>();

        // Apply effects to each unit and store actual damage
        foreach (var unit in affectedUnits)
        {
            Debug.Log($"Applying attack effects to {unit.name}");
            int actualDamage = ApplyAttackToUnit(unit);
            if (actualDamage > 0)
                unitDamageMap[unit] = actualDamage;
        }

        // ✅ FIXED: Update HUD with actual damage dealt (including criticals)
        foreach (var tile in area)
        {
            Vector2 center = tile.transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, 0.3f);
            foreach (var col in hits)
            {
                if (col.CompareTag("Enemy"))
                {
                    var u = col.GetComponent<Unit>();
                    var gabiteHUDObj = col.transform.Find("HUDSecundaria/GabiteHUD");
                    if (gabiteHUDObj != null)
                    {
                        var gabiteHUD = gabiteHUDObj.GetComponent<BattleHUD>();
                        if (gabiteHUD != null && !hudsToReset.Contains(gabiteHUD))
                        {
                            gabiteHUD.SetHUD(u);
                            gabiteHUD.Show();
                            hudsToReset.Add(gabiteHUD);
                        }
                    }
                }
            }
        }

    }

    int ApplyAttackToUnit(Unit targetUnit)          ///////////////////// OPTIMIZAR ESTO ///////////////////
    {
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
        float damageMultiplier = isCritical ? criticalMultiplier : 1.0f;                ////// Se podría optimizar creo

        // ✅ FIXED: Declare attackerStatusManager outside of damage block
        var attackerStatusManager = currentUnit.GetComponent<StatusEffectManager>();

        if (currentAttack.damage > 0)
        {
            int baseAttackDamage = currentAttack.damage + Mathf.RoundToInt(currentUnit.Fue * currentAttack.scalingFactor);

            if (attackerStatusManager != null)
            {
                float bonusPercent = attackerStatusManager.CalculateAttackBonusPercent();
                if (bonusPercent != 0f)
                {
                    baseAttackDamage = Mathf.RoundToInt(baseAttackDamage * (1f + bonusPercent));
                    baseAttackDamage = Mathf.Max(0, baseAttackDamage);
                    Debug.Log($"{currentUnit.name} attack bonus: +{bonusPercent * 100}% (base damage with bonus: {baseAttackDamage})");
                }
                
                float outgoingPenalty = attackerStatusManager.CalculateOutgoingDamagePenalty();
                if (outgoingPenalty > 0)
                {
                    baseAttackDamage = Mathf.RoundToInt(baseAttackDamage * (1f - outgoingPenalty));
                    baseAttackDamage = Mathf.Max(0, baseAttackDamage);
                    Debug.Log($"{currentUnit.name} damage reduced by {outgoingPenalty * 100}% due to status effects");
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
            foreach (var attackEffect in buffDebuffAttack.statusEffects)
            {
                if (Random.value <= attackEffect.probability)
                {
                    var statusManager = targetUnit.GetComponent<StatusEffectManager>();
                    if (statusManager == null)
                    {
                        statusManager = targetUnit.gameObject.AddComponent<StatusEffectManager>();
                    }

                    var effectToApply = attackEffect.statusEffect.Clone();
                    effectToApply.caster = currentUnit;
                    statusManager.ApplyEffect(effectToApply);
                }
            }

            // Push mechanics (only available for BuffDebuffAttackData)
            if (buffDebuffAttack.canPushTarget && Random.value < buffDebuffAttack.pushChance)
            {
                PushUnit(targetUnit, buffDebuffAttack.pushDistance);
            }
        }

        if (attackerStatusManager != null && currentAttack.damage > 0)
        {
            var effectsToTrigger = attackerStatusManager.GetActiveEffects()
                .Where(e => e.triggers.Contains(EffectTrigger.OnAttack))
                .ToList();

            foreach (var effect in effectsToTrigger)
            {
                attackerStatusManager.TriggerEffect(effect, EffectTrigger.OnAttack);
            }
        }

        // Return the final damage dealt for HUD updates
        if (currentAttack.damage > 0)
        {
            return finalDamage;
        }
        return 0;
    }



    // ✅ RESTORED: Push mechanics
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

    // ✅ RESTORED: Helper methods
    Vector2Int GetPushDirection(OverlayTile from, OverlayTile to)
    {
        Vector2Int delta = to.grid2DLocation - from.grid2DLocation;
        return new Vector2Int(Mathf.Clamp(delta.x, -1, 1), Mathf.Clamp(delta.y, -1, 1));
    }

    // ✅ FIXED: Safe property access in targeting methods
    bool ShouldAffectEnemies()
    {
        var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
        return buffDebuffAttack != null && buffDebuffAttack.statusEffects.Any(e => e.targetEnemies);
    }

    bool ShouldAffectAllies()
    {
        var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
        return buffDebuffAttack != null && buffDebuffAttack.statusEffects.Any(e => e.targetAllies);
    }

    bool ShouldAffectSelf()                                                     /////////////  Ver de Juntar los 3 metodos ///////////
    {
        var buffDebuffAttack = currentAttack as BuffDebuffAttackData;
        return buffDebuffAttack != null && buffDebuffAttack.statusEffects.Any(e => e.targetSelf);
    }

    // All the area calculation methods (GetEffectArea, facing methods, etc.)
    List<OverlayTile> GetEffectArea(OverlayTile center)                              //////// ver de optimizarlo un poco
    {
        var mapTiles = MapManager.Instance.map.Values;
        var area = new List<OverlayTile>();
        Vector2 origin = center.transform.position;
        float threshold = 0.1f;
        int size = currentAttack.areaSize;

        switch (currentAttack.effectShape)
        {
            case AreaShape.Circle:
                area = rangeFinder.GetTilesInRange(center, size);
                break;
            case AreaShape.LineHorizontal:
                float originThreshold = 0.1f;
                var playerTileH = MapManager.Instance.map.Values.FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < originThreshold);
                if (playerTileH != null)
                {
                    var facingH = DetermineFacingFromTiles(playerTileH, center);
                    area = GetDirectionalHorizontal(playerTileH, facingH, currentAttack.areaSize);
                }
                break;
            case AreaShape.LineVertical:
                float originThresholdV = 0.1f;
                var playerTileV = MapManager.Instance.map.Values
                    .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < originThresholdV);
                if (playerTileV != null)
                {
                    var facingV = DetermineFacingFromTiles(playerTileV, center);
                    area = GetDirectionalVertical(playerTileV, facingV, currentAttack.areaSize);
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
            case AreaShape.Boomerang:
                float OriginThreshold = 0.1f;
                var playerTile = MapManager.Instance.map.Values
                    .FirstOrDefault(t => Vector2.Distance((Vector2)t.transform.position, (Vector2)currentUnit.transform.position) < OriginThreshold);
                area = GetBoomerangArea(playerTile, center);
                break;
            case AreaShape.Perpendicular:
                area = rangeFinder.GetTilesInRange(center, size, currentAttack);
                break;
        }
        
        if (currentAttack.effectShape == AreaShape.Perpendicular)
        {
            Debug.Log($"[GetEffectArea] Perpendicular area tiles: {string.Join(", ", area.Select(t => $"({t.grid2DLocation.x},{t.grid2DLocation.y})"))}");
        }
        
        return area;
    }


    // All facing and directional methods
    Facing4 DetermineFacingFromTiles(OverlayTile playerTile, OverlayTile targetTile)
    {
        if (playerTile == null) return Facing4.ArribaDer;

        Vector2Int c = playerTile.grid2DLocation;
        Vector2Int t = (targetTile != null) ? targetTile.grid2DLocation : c + Vector2Int.right;
        Vector2Int delta = new Vector2Int(t.x - c.x, t.y - c.y);

        if (delta == Vector2Int.zero) delta = Vector2Int.right;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            return delta.x > 0 ? Facing4.ArribaDer : Facing4.AbajoIzq;
        }
        else
        {
            return delta.y > 0 ? Facing4.ArribaIzq : Facing4.AbajoDer;
        }
    }

    void ApplyFacingToUnit(Unit unit, Facing4 facing)
    {
        if (unit == null) return;

        var anim = unit.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            try { anim.SetInteger("FacingIndex", (int)facing); }
            catch { /* harmless if param missing */ }
        }

        var sr = unit.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            bool shouldFlip = (facing == Facing4.ArribaDer || facing == Facing4.AbajoDer);
            sr.flipX = shouldFlip;

            Transform effectsTransform = unit.transform.Find("Effects");
            if (effectsTransform != null)
            {
                var effectController = effectsTransform.GetComponent<EffectMirrorController>();
                if (effectController == null)
                {
                    effectController = effectsTransform.gameObject.AddComponent<EffectMirrorController>();
                }

                // Animation position/scale flip - original logic
                bool animationShouldFlip = (facing == Facing4.ArribaDer || facing == Facing4.AbajoDer);
                effectController.shouldMirrorAnimation = animationShouldFlip;

                // Sprite visual flip - NEW logic
                //bool spriteShouldFlip = (facing == Facing4.ArribaIzq || facing == Facing4.ArribaDer);
                //effectController.shouldFlipSprite = spriteShouldFlip;
            }

        }

    }








    Vector2Int FacingToVector(Facing4 f)
    {
        switch (f)
        {
            case Facing4.ArribaIzq: return new Vector2Int(0, +1);
            case Facing4.ArribaDer: return new Vector2Int(+1, 0);
            case Facing4.AbajoDer: return new Vector2Int(0, -1);
            case Facing4.AbajoIzq: return new Vector2Int(-1, 0);
            default: return new Vector2Int(+1, 0);
        }
    }

    Vector2Int Perpendicular(Vector2Int facing)
    {
        return new Vector2Int(facing.y, -facing.x);
    }

    List<OverlayTile> GetDirectionalVertical(OverlayTile playerTile, Facing4 facing, int size)
    {
        var grid = MapManager.Instance.map;
        var origin = playerTile.grid2DLocation;
        var dir = FacingToVector(facing);

        var area = new List<OverlayTile>();
        for (int i = 1; i <= size; i++)
        {
            var key = origin + dir * i;
            if (grid.TryGetValue(key, out var t) && t != null)
                area.Add(t);
        }
        return area;
    }

    List<OverlayTile> GetDirectionalHorizontal(OverlayTile playerTile, Facing4 facing, int size)
    {
        var grid = MapManager.Instance.map;
        var origin = playerTile.grid2DLocation;
        var dir = FacingToVector(facing);
        var perp = Perpendicular(dir);

        var area = new List<OverlayTile>();
        for (int i = -size; i <= size; i++)
        {
            var key = origin + dir + perp * i;
            if (grid.TryGetValue(key, out var t) && t != null)
                area.Add(t);
        }
        return area;
    }

    List<OverlayTile> GetBoomerangArea(OverlayTile center, OverlayTile target)
    {
        var result = new List<OverlayTile>();
        if (center == null) return result;
        if (MapManager.Instance == null || MapManager.Instance.map == null) return result;

        var baseOffsets = new List<Vector2Int>
        {
            new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,1),
            new Vector2Int(3,1), new Vector2Int(4,1), new Vector2Int(4,0),
            new Vector2Int(4,-1), new Vector2Int(3,-1), new Vector2Int(2,-1)
        };

        var facing = DetermineFacingFromTiles(center, target);
        List<Vector2Int> chosenOffsets;

        switch (facing)
        {
            case Facing4.ArribaDer:
                chosenOffsets = baseOffsets;
                break;
            case Facing4.AbajoIzq:
                chosenOffsets = baseOffsets.Select(o => new Vector2Int(-o.x, -o.y)).ToList();
                break;
            case Facing4.ArribaIzq:
                chosenOffsets = baseOffsets.Select(o => new Vector2Int(-o.y, o.x)).ToList();
                break;
            case Facing4.AbajoDer:
                chosenOffsets = baseOffsets.Select(o => new Vector2Int(o.y, -o.x)).ToList();
                break;
            default:
                chosenOffsets = baseOffsets;
                break;
        }

        Vector2Int c = center.grid2DLocation;
        foreach (var off in chosenOffsets)
        {
            var key = c + off;
            if (MapManager.Instance.map.TryGetValue(key, out var tile) && tile != null)
                result.Add(tile);
        }

        return result;
    }

    OverlayTile GetTileUnderCursor()
    {
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast((Vector2)wp, Vector2.zero);
        return hit.collider?.GetComponent<OverlayTile>();
    }

    void ClearPreview()
    {
        foreach (var t in previewTiles)
            t.HideTile();
        foreach (var t in validTiles)
            t.ShowOverlay(rangeColor);
        previewTiles.Clear();

        foreach (var hud in hudsToReset)
            hud.ResetPreview();
    }

    public void ExitAttackMode()
    {
        mouseController.attackBools.ResetAllSamuraiAttacks();
        mouseController.attackBools.ResetAllNinjaAttacks();
        mouseController.attackBools.ResetAllGeishaAttacks();
        mouseController.animatorSamurai.SetBool("idleBatallaUp", false);
        mouseController.animatorNinja.SetBool("idleBatallaUp", false);
        mouseController.animatorGeisha.SetBool("idleBatallaUp", false);
        mouseController.animatorSamurai.SetBool("idleBatalla", false);
        mouseController.animatorNinja.SetBool("idleBatalla", false);
        mouseController.animatorGeisha.SetBool("idleBatalla", false);

        if (!inAttackMode) return;

        inAttackMode = false;
        currentAttack = null;

        validTiles.ForEach(t => t.HideTile());
        previewTiles.ForEach(t => t.HideTile());
        previewTiles.Clear();
        validTiles.Clear();

        if (mouseController != null)
        {
            mouseController.enabled = true;
            mouseController.canAttack = true;
            mouseController.showPanelAcciones = true;

        }

        var panels = Object.FindObjectsByType<PanelAcciones>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var p in panels)
        {
            p.Show();
            p.panelBatalla.SetActive(false);

            var turnable = p.ownerCharacter.GetComponent<Turnable>();
            if (turnable != null && turnable.btnBatalla != null)
                turnable.btnBatalla.interactable = true;
        }

        var ataques = Object.FindObjectsByType<AttackButtonProxy>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );
        foreach (var a in ataques)
        {
            if (a.panelBatallaGeneral != null)
                a.panelBatallaGeneral.SetActive(true);
        }

        attackUI.SetButtonsInteractable(true);

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            var gabiteHUDObj = enemy.transform.Find("HUDSecundaria/GabiteHUD");
            if (gabiteHUDObj != null)
            {
                var gabiteHUD = gabiteHUDObj.GetComponent<BattleHUD>();
                if (gabiteHUD != null)
                    gabiteHUD.Hide();
            }

            var hitMarker = enemy.transform.Find("HitMarker");
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);
        }
    }

    IEnumerator ShowImpactAndFinish(List<OverlayTile> area)
    {
        area.ForEach(t => t.ShowOverlay(impactColor));

        // Initiative bonus handling
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
        attackBools.ResetDirectionStates();

        if (mouseController != null) mouseController.enabled = true;

        foreach (var enemyObj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            var hitMarker = enemyObj.transform.Find("HitMarker");
            if (hitMarker != null)
                hitMarker.gameObject.SetActive(false);
        }

        var ci = currentUnit.GetComponent<CharacterInfo>();
        bool hasMovesLeft = ci.tilesMoved < ci.maxTiles;

        if (hasMovesLeft)
        {
            mouseController.canMove = true;
            mouseController.canAttack = false;
            mouseController.showPanelAcciones = true;

            var panel = Object.FindObjectsByType<PanelAcciones>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ).FirstOrDefault(p => p.ownerCharacter == ci);

            if (panel != null)
            {
                panel.Show();
                panel.panelBatalla.SetActive(false);
            }

            attackUI.SetButtonsInteractable(true);
            attackExecuted = false;

            foreach (var hud in hudsToReset)
                hud?.Hide();
            hudsToReset.Clear();

            yield break;
        }

        mouseController.DeselectCharacter();
        attackUI.SetButtonsInteractable(true);
        attackExecuted = false;


        foreach (var hud in hudsToReset)
            hud?.Hide();
        hudsToReset.Clear();
    }

}

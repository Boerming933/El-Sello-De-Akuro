using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    [Header("Active Effects")]
    public List<StatusEffect> activeEffects = new List<StatusEffect>();


    public event Action<StatusEffect> OnEffectApplied;
    public event Action<StatusEffect> OnEffectRemoved;
    public event Action<StatusEffect> OnEffectTriggered;
    
    private Unit unit;
    private BattleSystem battleSystem;
    private Unit lastAttacker; // Track who last attacked this unit
    
    private void Awake()
    {
        unit = GetComponent<Unit>();
        battleSystem = FindFirstObjectByType<BattleSystem>();
        
        if (battleSystem != null)
        {
            battleSystem.OnTurnStart += HandleTurnStart;
        }
    }
    
    private void OnDestroy()
    {
        if (battleSystem != null)
        {
            battleSystem.OnTurnStart -= HandleTurnStart;
        }
    }
    
    public bool ApplyEffect(StatusEffect effect)
    {
        if (effect == null) return false;
        
        // Special case: Martial Rhythm on stunned ally
        if (effect.effectType == StatusEffectType.MartialRhythm && HasEffect(StatusEffectType.Stun))
        {
            RemoveEffect(StatusEffectType.Stun);
            effect = StatusEffectFactory.CreateMartialRhythmStunRemoval();
            Debug.Log($"{unit.name} had stun removed and gains enhanced Martial Rhythm!");
        }
        
        var existingEffect = activeEffects.FirstOrDefault(e => e.effectType == effect.effectType);
        
        if (existingEffect != null && !effect.canStack)
        {
            Debug.Log($"{unit.name} already has {effect.effectName}. Not stacking.");
            return false;
        }
        
        var newEffect = effect.Clone();
        newEffect.target = unit;
        newEffect.turnApplied = turnNumber;
        activeEffects.Add(newEffect);
        
        OnEffectApplied?.Invoke(newEffect);
        Debug.Log($"Applied {newEffect.effectName} to {unit.name} for {newEffect.duration} turns (skipTurn: {newEffect.skipTurn})");
        
        return true;
    }
    
    public bool RemoveEffect(StatusEffectType effectType)
    {
        var effect = activeEffects.FirstOrDefault(e => e.effectType == effectType);
        if (effect != null)
        {
            activeEffects.Remove(effect);
            OnEffectRemoved?.Invoke(effect);
            Debug.Log($"Removed {effect.effectName} from {unit.name}");
            return true;
        }
        return false;
    }
    
    public bool HasEffect(StatusEffectType effectType)
    {
        return activeEffects.Any(e => e.effectType == effectType && e.IsActive);
    }
    
    public StatusEffect GetEffect(StatusEffectType effectType)
    {
        return activeEffects.FirstOrDefault(e => e.effectType == effectType && e.IsActive);
    }
    
    private void HandleTurnStart(Unit currentUnit)
    {
        if (currentUnit != unit) return;
        
        // ✅ NEW: Increment turn number for this unit
        turnNumber++;
        
        var effectsToRemove = new List<StatusEffect>();
        
        foreach (var effect in activeEffects.ToList())
        {
            if (effect.decrementOnTurnStart)
            {
                effect.DecrementDuration();
                
                if (!effect.IsActive)
                {
                    effectsToRemove.Add(effect);
                }
            }
            
            if (effect.triggers.Contains(EffectTrigger.OnTurnStart))
            {
                TriggerEffect(effect, EffectTrigger.OnTurnStart);
            }
        }
        
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect.effectType);
        }
    }
    
    public void TriggerEffect(StatusEffect effect, EffectTrigger trigger)
    {
        if (!effect.triggers.Contains(trigger)) return;
        
        OnEffectTriggered?.Invoke(effect);
        
        switch (effect.effectType)
        {
            case StatusEffectType.Guard:
                HandleGuardTrigger(effect, trigger);
                break;
            case StatusEffectType.DraconicStance:
                HandleDraconicStanceTrigger(effect, trigger);
                break;
            case StatusEffectType.DamageBoost:  // ✅ ADD THIS
                HandleDamageBoostTrigger(effect, trigger);
                break;
        }
    }

    // ✅ ADD THIS METHOD
    private void HandleDamageBoostTrigger(StatusEffect effect, EffectTrigger trigger)
    {
        if (trigger == EffectTrigger.OnAttack)
        {
            Debug.Log($"{unit.name} consumed {effect.effectName} after attack!");
            RemoveEffect(effect.effectType);
        }
    }
    
    private void HandleGuardTrigger(StatusEffect effect, EffectTrigger trigger)
    {
        if (trigger == EffectTrigger.OnDamageReceived && lastAttacker != null)
        {
            int counterDamage = UnityEngine.Random.Range(effect.counterDamageMin, effect.counterDamageMax + 1);
            Debug.Log($"{unit.name} counters {lastAttacker.name} for {counterDamage} damage!");
            lastAttacker.TakeDamage(counterDamage);
        }
    }
    
    private void HandleDraconicStanceTrigger(StatusEffect effect, EffectTrigger trigger)
    {
        if (trigger == EffectTrigger.OnDamageReceived && lastAttacker != null)
        {
            int counterDamage = UnityEngine.Random.Range(effect.counterDamageMin, effect.counterDamageMax + 1);
            Debug.Log($"{unit.name} completely negates attack and counters {lastAttacker.name} for {counterDamage} damage!");
            lastAttacker.TakeDamage(counterDamage);
            
            // Apply "must attack next turn" effect
            var mustAttackEffect = new StatusEffect(StatusEffectType.DamageBoost, 1)
            {
                effectName = "Must Attack",
                mustAttackNextTurn = true,
                turnApplied = turnNumber // ✅ NEW: Track when this effect was applied
            };
            ApplyEffect(mustAttackEffect);
            
            // Remove Draconic Stance after use
            RemoveEffect(StatusEffectType.DraconicStance);
        }
    }
    
    // Damage calculation methods
    public float CalculateDamageReduction()
    {
        return activeEffects.Where(e => e.IsActive).Sum(e => e.damageReduction);
    }
    
    public float CalculateAttackBonusPercent()
    {
        float totalBonusPercent = 0f;
        foreach (var effect in activeEffects)
        {
            if (effect.IsActive)
            {
                totalBonusPercent += effect.attackBonusPercent;
            }
        }
        return totalBonusPercent;
    }

    public float CalculateOutgoingDamagePenalty()
    {
        float totalPenalty = 0f;
        foreach (var effect in activeEffects)
        {
            if (effect.IsActive)
            {
                totalPenalty += effect.attackReduction;
            }
        }
        return totalPenalty;
    }

    
    // Movement/action restriction checks
    public bool CanMove()
    {
        return !activeEffects.Any(e => e.IsActive && e.blockMovement);
    }
    
    public bool CanAttack()
    {
        return !activeEffects.Any(e => e.IsActive && e.blockAttack);
    }
    
    public bool ShouldSkipTurn()
    {
        bool shouldSkip = activeEffects.Any(e => e.IsActive && e.skipTurn);
        if (shouldSkip)
        {
            Debug.Log($"[StatusEffectManager] {unit.name} ShouldSkipTurn() = TRUE. Active skip effects:");
            foreach (var effect in activeEffects.Where(e => e.IsActive && e.skipTurn))
            {
                Debug.Log($"  - {effect.effectName} (duration: {effect.duration})");
            }
        }
        return shouldSkip;
    }
    
    public bool MustAttackNextTurn()
    {
        // ✅ NEW: Only enforce mustAttackNextTurn if it wasn't applied this turn
        return activeEffects.Any(e => e.IsActive && e.mustAttackNextTurn && e.turnApplied != GetCurrentTurnNumber());
    }
    
    // ✅ NEW: Clear the mustAttackNextTurn condition after attacking
    public void ClearMustAttackCondition()
    {
        var mustAttackEffects = activeEffects.Where(e => e.IsActive && e.mustAttackNextTurn).ToList();
        foreach (var effect in mustAttackEffects)
        {
            Debug.Log($"Clearing 'must attack' condition from {unit.name} - {effect.effectName}");
            RemoveEffect(effect.effectType);
        }
    }
    
    // ✅ NEW: Get a simple turn number based on how many turn starts this unit has experienced
    private int turnNumber = 0;
    
    private int GetCurrentTurnNumber()
    {
        return turnNumber;
    }
    
    public void SetLastAttacker(Unit attacker)
    {
        lastAttacker = attacker;
    }
    
    // Clear all effects (for end of battle)
    public void ClearAllEffects()
    {
        activeEffects.Clear();
        Debug.Log($"Cleared all status effects from {unit.name}");
    }

    private void Update()
{
    // Debug display active effects (remove this later)
    if (Input.GetKeyDown(KeyCode.F1) && activeEffects.Count > 0)
    {
        Debug.Log($"{unit.name} active effects:");
        foreach (var effect in activeEffects)
        {
            Debug.Log($"  - {effect.effectName}: {effect.duration} turns left, CanMove: {CanMove()}");
        }
    }
}

public List<StatusEffect> GetActiveEffects()
{
    return activeEffects.Where(e => e.IsActive).ToList();
}


}

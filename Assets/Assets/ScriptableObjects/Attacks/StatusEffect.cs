using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    Guard, DraconicStance, MartialRhythm, HypnoticChant, Stun, DamageBoost, MovementRestriction
}

public enum EffectTrigger
{
    OnTurnStart, OnTurnEnd, OnHit, OnDamageReceived, OnAttack, OnMove
}

[System.Serializable]
public class StatusEffect
{
    [Header("Effect Identity")]
    public StatusEffectType effectType;
    public string effectName;
    public string description;
    
    [Header("Duration & Stacking")]
    public int duration;
    public bool canStack = false;
    public bool decrementOnTurnStart = true;
    
    [Header("Effect Values")]
    public float damageReduction = 0f;
    public int counterDamageMin = 0;
    public int counterDamageMax = 0;
    public int attackBonus = 0;
    public float attackReduction = 0f;
    public bool blockMovement = false;
    public bool blockAttack = false;
    public bool skipTurn = false;
    public bool mustAttackNextTurn = false;
    
    [Header("Turn Tracking")]
    public int turnApplied = -1; // Track which turn this effect was applied
    
    [Header("Visual/UI Hooks - Set These Later")]
    public Sprite effectIcon;
    public GameObject visualEffectPrefab;
    public string animatorParameterName;
    public Color overlayColor = Color.white;
    
    [Header("Events")]
    public List<EffectTrigger> triggers = new List<EffectTrigger>();
    
    public bool IsActive => duration > 0;
    public Unit caster;
    public Unit target;
    
    public StatusEffect(StatusEffectType type, int dur)
    {
        effectType = type;
        duration = dur;
    }
    
    public void DecrementDuration()
    {
        if (duration > 0) duration--;
    }
    
    public StatusEffect Clone()
    {
        return (StatusEffect)this.MemberwiseClone();
    }
}

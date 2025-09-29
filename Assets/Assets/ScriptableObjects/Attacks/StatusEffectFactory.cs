using UnityEngine;

public static class StatusEffectFactory
{
    public static StatusEffect CreateKatanaGuard()
    {
        return new StatusEffect(StatusEffectType.Guard, 1)
        {
            effectName = "Katana Guard",
            description = "Reduces frontal damage by 50% and counters with 3-5 damage",
            damageReduction = 0.5f,
            counterDamageMin = 3,
            counterDamageMax = 5,
            triggers = { EffectTrigger.OnDamageReceived }
        };
    }
    
    public static StatusEffect CreateDraconicStance()
    {
        return new StatusEffect(StatusEffectType.DraconicStance, 999) // High duration as fallback
        {
            effectName = "Draconic Stance",
            description = "Negates next attack and counters with 6-8 damage",
            damageReduction = 1.0f,
            counterDamageMin = 6,
            counterDamageMax = 8,
            mustAttackNextTurn = true,
            decrementOnTurnStart = false, // ✅ Prevents auto-decrement - lasts until triggered
            triggers = { EffectTrigger.OnDamageReceived }
        };
    }
    
    public static StatusEffect CreateMartialRhythm()
    {
        return new StatusEffect(StatusEffectType.MartialRhythm, 1)
        {
            effectName = "Martial Rhythm",
            description = "Grants +1 attack damage for next turn",
            attackBonus = 1
        };
    }
    
    public static StatusEffect CreateMartialRhythmStunRemoval()
    {
        return new StatusEffect(StatusEffectType.MartialRhythm, 1)
        {
            effectName = "Martial Rhythm (Enhanced)",
            description = "Removes stun and grants +2 attack damage",
            attackBonus = 2
        };
    }
    
    public static StatusEffect CreateHypnoticChant()
    {
        return new StatusEffect(StatusEffectType.HypnoticChant, 1)
        {
            effectName = "Hypnotic Chant",
            description = "Cannot move but can still attack",
            blockMovement = true
        };
    }
    
    public static StatusEffect CreateStun()
    {
        return new StatusEffect(StatusEffectType.Stun, 1)
        {
            effectName = "Stunned",
            description = "Skips next turn",
            skipTurn = true
        };
    }
    
    public static StatusEffect CreatePreparedStrike()
    {
        return new StatusEffect(StatusEffectType.DamageBoost, 1)
        {
            effectName = "Prepared Strike",
            description = "Next attack deals +1 damage",
            attackBonus = 1
        };
    }
}
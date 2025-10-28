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
            description = "Grants +10% attack damage for next turn",
            attackBonusPercent = 0.10f
        };
    }
    
    public static StatusEffect CreateMartialRhythmStunRemoval()
    {
        return new StatusEffect(StatusEffectType.MartialRhythm, 1)
        {
            effectName = "Martial Rhythm (Enhanced)",
            description = "Removes stun and grants +20% attack damage",
            attackBonusPercent = 0.20f
        };
    }
    
    public static StatusEffect CreateHypnoticChant()
    {
        return new StatusEffect(StatusEffectType.HypnoticChant, 2)
        {
            effectName = "Canto Hipnótico",
            description = "No puede moverse y su daño se reduce 50%",
            blockMovement = true,
            attackReduction = 0.5f // 50% damage reduction
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
            description = "Next attack deals +50% damage",
            attackBonusPercent = 0.50f,
            triggers = { EffectTrigger.OnAttack }
        };
    }
}
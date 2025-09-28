using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackStatusEffect
{
    public StatusEffect statusEffect;
    public float probability = 1.0f; // 100% by default
    public bool targetSelf = false;
    public bool targetAllies = false;
    public bool targetEnemies = true;
}

public enum AttackType
{
    Damage, Buff, Debuff, Mixed
}

[CreateAssetMenu(fileName = "NewBuffDebuffAttack", menuName = "Combat/Buff Debuff Attack")]
public class BuffDebuffAttackData : AttackData
{
    [Header("Attack Classification")]
    public AttackType attackType = AttackType.Damage;
    
    [Header("Status Effects")]
    public List<AttackStatusEffect> statusEffects = new List<AttackStatusEffect>();
    
    [Header("Special Conditions")]
    public bool requiresBehindTarget = false;
    public bool requiresEmptyPath = false;
    public int maxUses = -1; // -1 for unlimited
    public int currentUses = 0;
    
    [Header("Positioning")]
    public bool canPushTarget = false;
    public float pushChance = 0.5f;
    public int pushDistance = 1;
    
    [Header("Critical Hit")]
    public float criticalChance = 0.0f;
    public float criticalMultiplier = 2.0f;
    
    public bool CanUse()
    {
        return maxUses == -1 || currentUses < maxUses;
    }
    
    public void UseAttack()
    {
        if (maxUses > 0) currentUses++;
    }
    
    public void ResetUses()
    {
        currentUses = 0;
    }
}

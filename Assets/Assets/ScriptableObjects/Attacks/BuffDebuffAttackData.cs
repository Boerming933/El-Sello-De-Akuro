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
    
    [Header("Turn-Based Cooldown")] //
    public int cooldownMax = 0; //
    
    private Dictionary<int, int> unitCooldowns = new Dictionary<int, int>(); //
    
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
    
    public bool IsOnCooldown(Unit unit) //
    { //
        if (unit == null || cooldownMax <= 0) return false; //
        int unitID = unit.GetInstanceID(); //
        return unitCooldowns.ContainsKey(unitID) && unitCooldowns[unitID] > 0; //
    } //
    
    public int GetRemainingCooldown(Unit unit) //
    { //
        if (unit == null || cooldownMax <= 0) return 0; //
        int unitID = unit.GetInstanceID(); //
        return unitCooldowns.ContainsKey(unitID) ? unitCooldowns[unitID] : 0; //
    } //
    
    public void StartCooldown(Unit unit) //
    { //
        if (unit == null || cooldownMax <= 0) return; //
        int unitID = unit.GetInstanceID(); //
        unitCooldowns[unitID] = cooldownMax; //
        Debug.Log($"[Cooldown] {attackName} cooldown started for {unit.Name}: {cooldownMax} turns"); //
    } //
    
    public void OnTurnPassed(Unit unit) //
    { //
        if (unit == null || cooldownMax <= 0) return; //
        int unitID = unit.GetInstanceID(); //
        if (unitCooldowns.ContainsKey(unitID) && unitCooldowns[unitID] > 0) //
        { //
            unitCooldowns[unitID]--; //
            Debug.Log($"[Cooldown] {attackName} cooldown for {unit.Name}: {unitCooldowns[unitID]} turns remaining"); //
            if (unitCooldowns[unitID] <= 0) //
            { //
                unitCooldowns.Remove(unitID); //
                ResetUses();
                Debug.Log($"[Cooldown] {attackName} is now available for {unit.Name}"); //
            } //
        } //
    } //
    
    public void ClearCooldown(Unit unit) //
    { //
        if (unit == null) return; //
        int unitID = unit.GetInstanceID(); //
        if (unitCooldowns.ContainsKey(unitID)) //
        { //
            unitCooldowns.Remove(unitID); //
            Debug.Log($"[Cooldown] {attackName} cooldown cleared for {unit.Name}"); //
        } //
    } //
}

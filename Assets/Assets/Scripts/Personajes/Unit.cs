using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public class Unit : MonoBehaviour
{
    [Header("Inventario de ataques")]
    public List<AttackData> allAttacks = new List<AttackData>();

    [Header("Ataques equipados")]
    public List<AttackData> equippedAttacks = new List<AttackData>();

    public int maxEquipped = 3;
    public string Name;
    public int Level;

    public bool isEnemy;

    public int Fue;
    public int movement;
    public int Des;
    public int Con;
    public int Int;
    public int maxHP;
    public int currentHP;
    public int maxMana;
    public int currentMana;
    public int pocionHeal;

    public Sprite portrait;
    public Sprite turnerIcon;

    public CharacterDetailsUI hud;

    public event Action<int> OnDamageTaken;
    public event Action OnDeath;

    private void Start()
    {
        // Ensure StatusEffectManager is present
        if (GetComponent<StatusEffectManager>() == null)
        {
            gameObject.AddComponent<StatusEffectManager>();
            Debug.Log($"Added StatusEffectManager to {Name}");
        }
        // Inicializa HUD al comenzar el combate
        hud.ShowDetails(this);
    }

    public bool EquipAttack(AttackData attack)
    {
        if (equippedAttacks.Contains(attack)) return false;
        if (equippedAttacks.Count >= maxEquipped) return false;
        equippedAttacks.Add(attack);
        return true;
    }

    public bool UnequipAttack(AttackData attack)
    {
        return equippedAttacks.Remove(attack);
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    //para enemigos

    public RaycastHit2D? ActiveTile()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
        return null;
    }

    //para jugadores

    public OverlayTile FindCenterTile()
    {
        const float threshold = 0.1f;
        return MapManager.Instance
            .map.Values
            .FirstOrDefault(t =>
                Vector2.Distance(
                    new Vector2(t.transform.position.x, t.transform.position.y),
                    new Vector2(transform.position.x, transform.position.y)
                ) < threshold
            );
    }

    public void Heal()
    {
        currentHP = Mathf.Min(currentHP + pocionHeal, maxHP);

        // Actualiza el HUD si está asignado
        if (hud != null)
            hud.ShowDetails(this);

        // fuerza actualización general
        var ui = UnityEngine.Object.FindFirstObjectByType<CharacterDetailsUI>();
        if (ui != null)
            ui.UpdateAllUI();

        Debug.Log($"{Name} curó {pocionHeal} HP. Vida actual: {currentHP}/{maxHP}");
    }

    public void TakeDamage(int amount, Unit attacker = null)
    {
        // Get status effect manager
        var statusManager = GetComponent<StatusEffectManager>();
        if (statusManager == null)
        {
            statusManager = gameObject.AddComponent<StatusEffectManager>();
        }
        
        // Set who attacked us for counter effects
        if (attacker != null)
        {
            statusManager.SetLastAttacker(attacker);
        }
        
        // Check for damage negation (Draconic Stance)
        if (statusManager.HasEffect(StatusEffectType.DraconicStance))
        {
            Debug.Log($"{Name} negates all damage with Draconic Stance!");
            statusManager.TriggerEffect(statusManager.GetEffect(StatusEffectType.DraconicStance), EffectTrigger.OnDamageReceived);
            return; // No damage taken
        }
        
        // Apply damage reduction (Guard)
        float damageReduction = statusManager.CalculateDamageReduction();
        int finalDamage = Mathf.RoundToInt(amount * (1f - damageReduction));
        
        // Apply damage
        currentHP = Mathf.Max(0, currentHP - finalDamage);
        
        // Trigger counter effects if we took damage
        if (finalDamage > 0 && statusManager.HasEffect(StatusEffectType.Guard))
        {
            statusManager.TriggerEffect(statusManager.GetEffect(StatusEffectType.Guard), EffectTrigger.OnDamageReceived);
        }
        
        // Update HUD
        var ui = UnityEngine.Object.FindFirstObjectByType<CharacterDetailsUI>();
        if (ui != null)
            ui.UpdateAllUI();
            
        // Dispatch events
        OnDamageTaken?.Invoke(finalDamage);
        
        // Check death
        if (currentHP == 0)
            Die();
    }
}
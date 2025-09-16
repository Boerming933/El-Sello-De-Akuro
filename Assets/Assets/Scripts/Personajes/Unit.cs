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

    public Sprite portrait;

    public CharacterDetailsUI hud;

    public event Action<int> OnDamageTaken;
    public event Action OnDeath;

    private void Start()
    {
        // Inicializa HUD al comenzar el combate
        hud.ShowDetails(this);
    }

    public void TakeDamage(int amount)
    {
        // 1) Ajusta HP
        currentHP = Mathf.Max(0, currentHP - amount);

        // 2) Actualiza HUD
        var ui = UnityEngine.Object.FindFirstObjectByType<CharacterDetailsUI>();
        if (ui != null)
            ui.UpdateAllUI();
        // 3) Dispara evento de daño
        OnDamageTaken?.Invoke(amount);

        // 4) Comprueba muerte
        if (currentHP == 0)
            Die();
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
        OnDeath?.Invoke();
        // GetComponent<Animator>()?.SetTrigger("Die");
        // collider.enabled = false;
        // this.enabled = false;
    }
    
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
}

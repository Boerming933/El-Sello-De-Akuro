using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;

public class Unit : MonoBehaviour
{
    [Header("Inventario de ataques")]
    public List<AttackData> allAttacks = new List<AttackData>();

    [Header("Ataques equipados")]
    public List<AttackData> equippedAttacks = new List<AttackData>();

    public Animator animator;
    public SpriteRenderer sr;

    public int maxEquipped = 3;
    public string Name;
    public int Level;

    public bool isEnemy;
    
    public float[] deathXCoords;
    public float[] deathYCoords;

    private Vector3 deathPosition;

    public GameObject hudCanva;

    public float[] finalXDistance;
    public float[] finalYDistance;

    private bool battleEnd = false;

    public float speed = 4f;
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
    
    public OverlayTile active;
    
    [HideInInspector] public bool MovimientoRelampagoCaminata = false; //

    public event Action<int> OnDamageTaken;
    public event Action OnDeath;
    
    public MouseControler mouseControler;
    public BattleSystem battleSystem;

    private void Start()
    {
        // Ensure StatusEffectManager is present
        if (GetComponent<StatusEffectManager>() == null)
        {
            gameObject.AddComponent<StatusEffectManager>();
            Debug.Log($"Added StatusEffectManager to {Name}");
        }

        battleSystem = BattleSystem.Instance.GetComponent<BattleSystem>();

        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

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

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.75f);

        if (isEnemy)
        {
            float best = int.MaxValue;
            int ind = 0;

            for (int i = 0; i < deathXCoords.Count(); i++)
            {
                var activePosition = active.transform.position;
                float d = Mathf.Abs(activePosition.x - deathXCoords[i]) + Mathf.Abs(activePosition.y - deathYCoords[i]);
                if (d < best)
                {
                    best = d;
                    ind = i;
                }
            }

            deathPosition = new Vector3(deathXCoords[ind], deathYCoords[ind], 0f);

            Debug.LogError("La mejor posicion de muerte es " + deathPosition);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (isEnemy)
        {
            var activePosition = ActiveTile();
            active = activePosition.Value.collider.GetComponent<OverlayTile>();
        }
        else
        {
            active = FindCenterTile();
        }

        var comparation = new Vector3(0f, 0f, 0f);

        if (deathPosition != comparation)
        {
            if (Name == "Oni1")
            {
                animator.SetTrigger("escape");
                if (deathPosition.x > transform.position.x)
                {
                    sr.flipX = true;
                }
                else sr.flipX = false;
            }
            else if (Name == "Oni2")
            {
                animator.SetTrigger("escape");
                if (deathPosition.x > transform.position.x)
                {
                    sr.flipX = true;
                }
                else sr.flipX = false;
            }
            else if (Name == "Oni3")
            {
                animator.SetTrigger("escape");
                if (deathPosition.x > transform.position.x)
                {
                    sr.flipX = true;
                }
                else sr.flipX = false;
            }

            transform.position = Vector2.MoveTowards(transform.position, deathPosition, speed * Time.deltaTime);
        }
        if (transform.position == deathPosition) Destroy(gameObject);

        if (battleSystem.BattleOver())
        {
            StartCoroutine(EndCombat());       
        }
    }

    private IEnumerator EndCombat()
    {
        yield return new WaitForSeconds(2f);

        var FirstMovement = new Vector3(finalXDistance[0], finalYDistance[0], transform.position.z);

        foreach (Transform children in transform)
        {
            children.gameObject.SetActive(false);
        }
        hudCanva.SetActive(false);

        if (transform.position.x < FirstMovement.x)
        {
            sr.flipX = true;
            animator.SetBool("isMovingDown", true);
        }
        else
        {
            sr.flipX = false;
            animator.SetBool("isMovingDown", true);
        }

        transform.position = Vector2.MoveTowards(transform.position, FirstMovement, speed * Time.deltaTime);

        if (transform.position == FirstMovement)
        {
            StartCoroutine(waitUntilEndPosition());
        }
    }
    
    IEnumerator waitUntilEndPosition()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("isMovingDown", false);
        animator.SetBool("isMovingUp", false);
        if (Name == "Riku Takeda") sr.flipX = true;
        else sr.flipX = false;
        if (Name == "Riku Takeda") mouseControler.FinalDialogue(this);
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

    public void TakeDamage(int amount, Unit attacker = null, bool skipStatusEffects = false)
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

        // Only process status effects if not already handled by attack controller
        if (!skipStatusEffects)
        {
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
        }
        else
        {
            // Just apply the damage without processing effects
            currentHP = Mathf.Max(0, currentHP - amount);
        }

        // Update HUD
        var ui = UnityEngine.Object.FindFirstObjectByType<CharacterDetailsUI>();
        if (ui != null)
            ui.UpdateAllUI();

        // Dispatch events
        OnDamageTaken?.Invoke(amount);

        // Check death
        if (currentHP == 0) StartCoroutine(Die());
    }

    public void GainMana()
    {
        if (currentMana <= maxMana - 2)
        {
            currentMana = currentMana + 2;
            // Actualiza el HUD si está asignado
            if (hud != null)
                hud.ShowDetails(this);
            // fuerza actualización general
            var ui = UnityEngine.Object.FindFirstObjectByType<CharacterDetailsUI>();
            if (ui != null)
                ui.UpdateAllUI();
            Debug.Log("MANA CONSEGUIDO");
        }

    }
}

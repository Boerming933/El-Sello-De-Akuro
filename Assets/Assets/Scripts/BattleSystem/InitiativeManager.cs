using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitiativeManager : MonoBehaviour
{
    [Tooltip("Unidades aliadas y enemigas que participan en la ronda")]
    public List<Unit> allies;
    public List<Unit> enemies;

    // Buffs temporales de iniciativa
    private class InitiativeBuff
    {
        public Unit unit;
        public int bonus;
        public int remainingTurns;
    }

    // Lista de buffs activos
    private List<InitiativeBuff> activeBuffs = new List<InitiativeBuff>();

    // Orden final de la iniciativa
    private List<InitiativeEntry> turnOrder = new List<InitiativeEntry>();
    private int currentIndex = 0;

    // Trackers para la lógica de ronda
    private int lastIndex         = -1;
    private bool reorderPending   = false;


    /// <summary>
    /// Llamar al inicio de la batalla para barajar la iniciativa.
    /// Incluye cualquier buff activo desde antes.
    /// </summary>
    public void RollInitiative()
    {
        turnOrder.Clear();
        reorderPending = false;

        // 1) Reunir participantes
        var participants = allies.Concat(enemies);

        // 2) Crear entradas iniciales
        foreach (var u in participants)
        {
            var entry = new InitiativeEntry
            {
                unit = u,
                dexterity = u.Des,
                roll = Random.Range(1, 21)
            };
            entry.total = entry.dexterity
                        + entry.roll
                        + GetBuffSum(u);

            turnOrder.Add(entry);
        }

        // 3) Ordenar
        SortTurnOrder();
        currentIndex = 0;
        lastIndex    = -1;
    }

    /// <summary>
    /// Devuelve la siguiente unidad en el turno, avanza índice
    /// y, si arrancamos nueva ronda, actualiza buffs y reordena.
    /// </summary>
    public Unit GetNextUnit()
    {
        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("InitiativeManager: no hay turnOrder. Llama a RollInitiative primero.");
            return null;
        }

        // Detectar inicio de nueva ronda completa:
        bool isNewRound = (currentIndex == 0 && lastIndex == turnOrder.Count - 1);
        if (isNewRound)
        {
            Debug.Log("[InitiativeManager] --- Nueva ronda empezada ---");
            UpdateBuffs();  

            if (reorderPending)
            {
                Debug.Log("[InitiativeManager] Reordenando al final de ronda por buffs pendientes");
                RecalculateAndKeepCurrent();
                reorderPending = false;
                Debug.Log("[InitiativeManager] Nuevo orden: " + FormatTurnOrder());
            }
        }

        var entry = turnOrder[currentIndex];
        Debug.Log($"[InitiativeManager] Turno {currentIndex+1}: {entry.unit.name} (Total: {entry.total})");

        lastIndex    = currentIndex;
        currentIndex = (currentIndex + 1) % turnOrder.Count;
        return entry.unit;
    }

    /// <summary>
    /// Aplica un bonus temporal de iniciativa a una unidad
    /// y reordena inmediatamente el turnero.
    /// </summary>
    public void ApplyInitiativeBuff(Unit u, int bonus, int durationTurns)
    {
        var buff = new InitiativeBuff
        {
            unit = u,
            bonus = bonus,
            remainingTurns = durationTurns
        };
        activeBuffs.Add(buff);
        reorderPending = true;
        Debug.Log($"[InitiativeBuff] Aplicado a {u.name}: +{bonus} ini por {durationTurns} turnos."
            + $" Total buffs ahora: {activeBuffs.Count}");
        //RecalculateAndKeepCurrent();
    }

    /// <summary>
    /// Reduce 1 ronda a cada buff y elimina los expirados.
    /// </summary>
    private void UpdateBuffs()
    {
        foreach (var b in activeBuffs)
            b.remainingTurns--;

        foreach (var b in activeBuffs)
            Debug.Log($"[InitiativeBuff] {b.unit.name} queda con {b.remainingTurns} ronda(s)");

        int removed = activeBuffs.RemoveAll(b => b.remainingTurns <= 0);
        if (removed > 0)
            Debug.Log($"[InitiativeBuff] Expiraron {removed} buff(s). Quedan: {activeBuffs.Count}");
    }

    /// <summary>
    /// Calcula el total de bonus de iniciativa activos para la unidad.
    /// </summary>
    private int GetBuffSum(Unit u)
    {
        return activeBuffs
            .Where(b => b.unit == u)
            .Sum(b => b.bonus);
    }

    /// <summary>
    /// Recalcula totales y reordena manteniendo el flujo
    /// (sitúa currentIndex justo tras la última unidad procesada).
    /// </summary>
    private void RecalculateAndKeepCurrent()
    {
        if (turnOrder.Count == 0) return;

        // Identificar última unidad jugada
        int lastIdx   = (lastIndex + turnOrder.Count) % turnOrder.Count;
        Unit lastUnit = turnOrder[lastIdx].unit;

        // Recalcular todos los totales
        foreach (var e in turnOrder)
            e.total = e.dexterity + e.roll + GetBuffSum(e.unit);

        SortTurnOrder();

        // Ajustar índice al siguiente de lastUnit
        int idx = turnOrder.FindIndex(e => e.unit == lastUnit);
        currentIndex = (idx + 1) % turnOrder.Count;
    }

    // Helper para formatear la lista en un string
    private string FormatTurnOrder()
    {
        return string.Join(" | ",
            turnOrder.Select(e =>
                $"{e.unit.name}(tot:{e.total})"
            )
        );
    }

    /// <summary>
    /// Ordena turnOrder por total, dexterity y roll para desempates.
    /// </summary>
    private void SortTurnOrder()
    {
        turnOrder = turnOrder
            .OrderByDescending(e => e.total)
            .ThenByDescending(e => e.dexterity)
            .ThenByDescending(e => e.roll)
            .ToList();
    }

    /// <summary>
    /// Opcional: re-calcula iniciativa al completar un ciclo de todos los participantes.
    /// </summary>
    public bool IsNewRound()
    {
        return currentIndex == 0;
    }

    /// <summary>
    /// Para debug: lista en consola orden de iniciativa.
    /// </summary>
    public void PrintTurnOrder()
    {
        Debug.Log("=== Turn Order ===");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            var e = turnOrder[i];
            Debug.Log($"{i + 1}. {e.unit.name} (Roll: {e.roll}, Dex: {e.dexterity}, Total: {e.total})");
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PrintTurnOrder();
        }
    }
}

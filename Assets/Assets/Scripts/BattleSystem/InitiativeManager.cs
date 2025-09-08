using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitiativeManager : MonoBehaviour
{
    [Tooltip("Unidades aliadas y enemigas que participan en la ronda")]
    public List<Unit> allies;
    public List<Unit> enemies;

    // Orden final de la iniciativa
    private List<InitiativeEntry> turnOrder = new List<InitiativeEntry>();
    private int currentIndex = 0;

    /// <summary>
    /// Llamar al inicio de la batalla para barajar la iniciativa.
    /// </summary>
    public void RollInitiative()
    {
        turnOrder.Clear();

        // 1) Reunir participantes
        var participants = new List<Unit>();
        participants.AddRange(allies);
        participants.AddRange(enemies);

        // 2) Rellenar la lista de entradas con tirada y total
        foreach (var u in participants)
        {
            var entry = new InitiativeEntry {
                unit      = u,
                dexterity = u.Des,                     // campo de destreza en Unit
                roll      = Random.Range(1, 21),              // d20
            };
            entry.total = entry.dexterity + entry.roll;
            turnOrder.Add(entry);
        }

        // 3) Ordenar de mayor a menor (si empatan, mayor destreza gana; sigue aleatorio)
        turnOrder = turnOrder
            .OrderByDescending(e => e.total)
            .ThenByDescending(e => e.dexterity)
            .ThenByDescending(e => e.roll)  // desempate extra
            .ToList();

        currentIndex = 0;
    }

    /// <summary>
    /// Devuelve la siguiente unidad en turnOrder y avanza el índice.
    /// </summary>
    public Unit GetNextUnit()
    {
        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("InitiativeManager: no hay turnOrder. Llamar a RollInitiative primero.");
            return null;
        }

        var entry = turnOrder[currentIndex];
        currentIndex = (currentIndex + 1) % turnOrder.Count;
        return entry.unit;
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
}

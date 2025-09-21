using UnityEngine;

public class InitiativeEntry
{
    public Unit unit;
    public int dexterity;
    public int roll;
    public int total;

    // Ronda mínima en la que puede jugar (compatibilidad con la lógica previa)
    public int spawnEligibleRound = 0;

    // --- Nuevo: bloqueo hasta que "blockingUnit" juegue otra vez ---
    // Si blockingUnit != null, esta entry queda bloqueada hasta que
    // playCounts[blockingUnit] > blockingUnitPlayCountAtSpawn
    public Unit blockingUnit = null;
    public int blockingUnitPlayCountAtSpawn = 0;
}


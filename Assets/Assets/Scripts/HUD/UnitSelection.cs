using UnityEngine;
using System;

public static class UnitSelection
{
    // Evento que pasa la unidad seleccionada a los listeners
    public static event Action<Unit> OnUnitSelected;

    // Llamar este método para disparar el evento
    public static void Select(Unit unit)
    {
        OnUnitSelected?.Invoke(unit);
    }
}

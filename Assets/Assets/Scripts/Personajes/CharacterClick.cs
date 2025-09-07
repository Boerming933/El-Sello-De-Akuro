using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterClick : MonoBehaviour, IPointerClickHandler
{
    public Unit unitData; // Referencia a la clase que contiene stats, nivel, etc.

    public void OnPointerClick(PointerEventData eventData)
    {
        // Dispara el evento centralizado
        UnitSelection.Select(unitData);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class AttackSelectionUI : MonoBehaviour
{
    public Unit playerUnit;                    // referencia al Unit
    public Transform contentParent;            // apunta a AttackPanel
    public AttackButtonHUD buttonPrefab;        // prefab configurado

    private List<AttackButtonHUD> equippedButtons = new();

    void Start()
    {
        RefreshCatalog();
        RefreshEquipped();
    }

    void RefreshCatalog()
    {
        // Limpia catálogo
        foreach (Transform c in contentParent) Destroy(c.gameObject);

        // Crea botón por cada ataque disponible
        foreach (var atk in playerUnit.allAttacks)
        {
            var btn = Instantiate(buttonPrefab, contentParent);
            btn.Setup(atk, this);
        }
    }

    void RefreshEquipped()
    {
        // Aquí podrías mostrar en otra parte de la UI los equipados
        // O aplicar feedback en los botones del catálogo
    }

    public void ToggleAttack(AttackData atk)
    {
        if (playerUnit.equippedAttacks.Contains(atk))
            playerUnit.UnequipAttack(atk);
        else
            playerUnit.EquipAttack(atk);

        RefreshEquipped();
    }
}

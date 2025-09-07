using UnityEngine;
using System.Collections.Generic;
using System;

public class AttackSelectionUI : MonoBehaviour
{
    public Unit playerUnit;                    // referencia al Unit
    public Transform contentParent;            // apunta a AttackPanel
    public AttackButtonHUD buttonPrefab;        // prefab configurado

    private List<AttackButtonHUD> equippedButtons = new();
    public event Action<AttackData> OnAttackChosen;

    void Start()
    {
        RefreshCatalog();
        RefreshEquipped();
    }

    void RefreshCatalog()
    {
        // // Limpia catálogo
        // foreach (Transform c in contentParent) Destroy(c.gameObject);

        // // Crea botón por cada ataque disponible
        // foreach (var atk in playerUnit.allAttacks)
        // {
        //     var btn = Instantiate(buttonPrefab, contentParent);
        //     btn.Setup(atk, this);
        // }
    }

    void RefreshEquipped()
    {
        foreach (Transform c in contentParent)
            Destroy(c.gameObject);

        // Crea hasta max 3 botones
        int max = Mathf.Min(playerUnit.equippedAttacks.Count, playerUnit.maxEquipped);
        for (int i = 0; i < max; i++)
        {
            var atk = playerUnit.equippedAttacks[i];
            var btn = Instantiate(buttonPrefab, contentParent);
            // Aquí pasamos el callback del propio UI
            btn.Setup(atk, this, attack => OnAttackChosen?.Invoke(attack));
        }
    }

    public void ToggleAttack(AttackData atk)
    {
        if (playerUnit.equippedAttacks.Contains(atk))
            playerUnit.UnequipAttack(atk);
        else
            playerUnit.EquipAttack(atk);

        RefreshEquipped();
    }
    
    public void SetButtonsInteractable(bool state)
    {
        foreach (var btn in equippedButtons)
            btn.SetInteractable(state);
    }
}

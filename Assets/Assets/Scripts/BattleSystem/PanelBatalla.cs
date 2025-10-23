using UnityEngine;
using UnityEngine.UI;

public class PanelBatalla : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The owning character's MouseController")]
    public MouseControler mouseController;

    public void RefreshButtonsBasedOnMana()
    {
        if (mouseController == null)
        {
            Debug.LogWarning("[PanelBatalla] MouseController is not assigned!");
            return;
        }

        Unit activeUnit = mouseController.myUnit;
        if (activeUnit == null)
        {
            Debug.LogWarning("[PanelBatalla] Active unit is null!");
            return;
        }

        int currentMana = activeUnit.currentMana;

        AttackButtonProxy[] attackButtons = GetComponentsInChildren<AttackButtonProxy>(true);

        foreach (var proxy in attackButtons)
        {
            if (proxy.attackData == null) continue;

            Button btn = proxy.GetComponent<Button>();
            if (btn == null) continue;

            bool canAfford = currentMana >= proxy.attackData.manaCost;
            btn.interactable = canAfford;

            if (!canAfford)
            {
                Debug.Log($"[PanelBatalla] Button {proxy.gameObject.name} disabled. Cost: {proxy.attackData.manaCost}, Available: {currentMana}");
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrderDisplay : MonoBehaviour
{
    [Header("Referencias")]
    public Image[] turnOrderImages; // Array de 6 imágenes del turnero (asignar en el inspector)

    private InitiativeManager initiativeManager;

    void Start()
    {
        initiativeManager = FindAnyObjectByType<InitiativeManager>();
        UpdateTurnOrder();
    }

    public void UpdateTurnOrder()
    {
        if (initiativeManager == null || initiativeManager.TurnOrder == null || initiativeManager.TurnOrder.Count == 0)
        {
            Debug.LogWarning("TurnOrderDisplay: No hay turnos disponibles.");
            return;
        }

        List<InitiativeEntry> turnQueue = initiativeManager.TurnOrder;
        int currentIndex = initiativeManager.CurrentIndex;

        for (int i = 0; i < turnOrderImages.Length; i++)
        {
            int index = (currentIndex + i - 1 + turnQueue.Count) % turnQueue.Count;
            InitiativeEntry entry = turnQueue[index];

            if (entry.unit != null && entry.unit.portrait != null)
            {
                turnOrderImages[i].sprite = entry.unit.turnerIcon;
                turnOrderImages[i].enabled = true;
            }
            else
            {
                turnOrderImages[i].sprite = null;
                turnOrderImages[i].enabled = false;
            }
        }
    }
}
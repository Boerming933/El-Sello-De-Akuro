using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttackButtonProxy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Tooltip("Drag here the AttackData ScriptableObject for this button")]
    public AttackData attackData;

    [Tooltip("Reference to your scene's AttackController")]
    public BuffDebuffAttackController attackController;

    [Tooltip("Panel de batalla (opcional) que quieras ocultar al hacer click")]
    public GameObject panelBatalla;

    public GameObject panelBatallaGeneral;

    public Button botonBatalla;

    public GameObject descripcionAtaque;
    public MouseControler mouseController;

    [Tooltip("Panel de acciones (optional) con el script PanelAcciones")]
    public PanelAcciones panelAcciones;

    public bool botonDesactivado = false;


    /// <summary>
    /// Asignar este método al OnClick() de tu botón en el Inspector.
    /// </summary>
    /// 

    void Start()
    {
        mouseController = Object.FindFirstObjectByType<MouseControler>();
    }
    void Update()
    {
        // Corrected the syntax for accessing the Button component and setting interactable
        Button buttonComponent = gameObject.GetComponent<Button>();
        if (buttonComponent != null)
        {
            if (attackData.manaCost > mouseController.myUnit.currentMana)
            {
                buttonComponent.interactable = false;
            }
            var bd = attackData as BuffDebuffAttackData;
            if (bd.maxUses != -1 && bd.currentUses >= bd.maxUses)
            {
                buttonComponent.interactable = false;
            }
            else
            {
                buttonComponent.interactable = true;
            }
        }
    }


    public void OnClick()
    {
        botonDesactivado = true;
        // 1) Ocultar paneles
        if (panelBatalla != null)
            panelBatalla.SetActive(false);

        if (panelBatallaGeneral != null)
            panelBatallaGeneral.SetActive(false);

        if (panelAcciones != null)
            panelAcciones.Hide();

        if (botonBatalla != null)
        {
            botonBatalla.interactable = false;
        }

        if (botonDesactivado)
        {
            botonBatalla.interactable = false;
        }

        // 2) Disparar la lógica de ataque
        if (attackController != null && attackData != null)
            attackController.StartAttack(attackData);

        HideDescription();

       
    }


    // Called when button becomes highlighted (mouse hover OR keyboard/gamepad selection)
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowDescription();
        AudioManager.Instance.PlaySFX("UI");
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowDescription();
        AudioManager.Instance.PlaySFX("UI");
    }

    // Called when button loses highlight (mouse exit OR keyboard/gamepad deselection)
    public void OnPointerExit(PointerEventData eventData)
    {
        HideDescription();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HideDescription();
    }

    private void ShowDescription()
    {
        if (descripcionAtaque != null)
            descripcionAtaque.SetActive(true);
    }

    private void HideDescription()
    {
        if (descripcionAtaque != null)
            descripcionAtaque.SetActive(false);
    }


    public void ShowGeneralBattlePanel()
    {
        if (panelBatallaGeneral != null)
            panelBatallaGeneral.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.UI;
using System;

public class AttackButtonHUD : MonoBehaviour
{
    //public Image iconImage;
    public Text nameText;
    private AttackData attackData;
    private AttackSelectionUI selectionUI;

    private Action<AttackData> onClickCallback;

    private bool isInteractable = true;
    private Button btn;

    void Awake()
    {
        // Si tienes un Button en el prefab, lo capturas aquí
        if (btn == null)
            btn = GetComponent<Button>();
    }

    public void Setup(AttackData data, AttackSelectionUI ui, System.Action<AttackData> callback)
    {
        attackData = data;
        //iconImage.sprite = data.icon;
        nameText.text = data.attackName;
        selectionUI = ui;
        onClickCallback = callback;

        btn.onClick.RemoveAllListeners();
        //btn.onClick.AddListener(() => onClickCallback?.Invoke(attackData));
        btn.onClick.AddListener(() => TryInvoke());
    }

    private void TryInvoke()
    {
        if (!isInteractable) return;
        onClickCallback?.Invoke(attackData);
    }

    public void OnClick()
    {
        selectionUI.ToggleAttack(attackData);
    }

    public void SetInteractable(bool state)
    {
        isInteractable = state;
        if (btn != null)
            btn.interactable = state;
        // si no tienes Button, aquí podrías cambiar color u otro feedback
    }
}

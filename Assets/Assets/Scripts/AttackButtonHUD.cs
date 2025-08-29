using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackButtonHUD : MonoBehaviour
{
    //public Image iconImage;
    public Text nameText;
    private AttackData attackData;
    private AttackSelectionUI selectionUI;

    public void Setup(AttackData data, AttackSelectionUI ui)
    {
        attackData = data;
        //iconImage.sprite = data.icon;
        nameText.text = data.attackName;
        selectionUI = ui;
    }

    public void OnClick()
    {
        selectionUI.ToggleAttack(attackData);
    }
}

using UnityEngine;

public class UnitHUD : MonoBehaviour
{
    public BattleHUD hud;
    private Unit unitData;

    void Awake()
    {
        unitData = GetComponent<Unit>();
        hud.Hide();
    }

    void OnMouseEnter()
    {
        hud.Show();
        hud.SetHUD(unitData);
        hud.SetHP(unitData.currentHP);
        hud.SetMana(unitData.currentMana);
    }

    void OnMouseExit()
    {
        hud.Hide();
    }
    void OnMouseUpAsButton()
    {
        UnitSelection.Select(unitData);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{

    public Text NumFue;
    public Text NumDes;
    public Text NumCon;
    public Text NumInt;

    public Slider HPSlider;
    public Slider ManaSlider;

    public void SetHUD(Unit unit)
    {
        NumFue.text = unit.Fue.ToString();
        NumDes.text = unit.Des.ToString();
        NumCon.text = unit.Con.ToString();
        NumInt.text = unit.Int.ToString();
        HPSlider.maxValue = unit.maxHP;
        HPSlider.value = unit.currentHP;

    }

    public void SetHP(int hp)
    {
        HPSlider.value = hp;
    }
    public void SetMana(int Mana)
    {
        ManaSlider.value = Mana;
    }

    public void Show()  => gameObject.SetActive(true);
    public void Hide()  => gameObject.SetActive(false);

}

using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    public Text NumFue;
    public Text NumDes;
    public Text NumCon;
    public Text NumInt;

    public Slider HPSlider;           // Vida real
    public Slider PreviewHPSlider;    // Vida previsualizacion
    public Slider ManaSlider;

    private void Update()
    {
        if (HPSlider.value <= 0)
        {
            HPSlider.fillRect.gameObject.SetActive(false);
            PreviewHPSlider.fillRect.gameObject.SetActive(false);
        }
        else
        {
            HPSlider.fillRect.gameObject.SetActive(true);
            PreviewHPSlider.fillRect.gameObject.SetActive(true);
        }

        if (PreviewHPSlider.value <= 0)
        {
            PreviewHPSlider.fillRect.gameObject.SetActive(false);
        }
        else
        {
            PreviewHPSlider.fillRect.gameObject.SetActive(true);
        }
    }
    public void SetHUD(Unit unit)
    {
        //NumFue.text = unit.Fue.ToString();
        //NumDes.text = unit.Des.ToString();
        //NumCon.text = unit.Con.ToString();
        //NumInt.text = unit.Int.ToString();

        HPSlider.maxValue = unit.maxHP;
        HPSlider.value = unit.currentHP;


        if (PreviewHPSlider != null)
        {
            PreviewHPSlider.maxValue = unit.maxHP;
            PreviewHPSlider.value = unit.currentHP;
        }

        ManaSlider.maxValue = unit.maxMana;
        ManaSlider.value = unit.currentMana;
    }

    public void SetHP(int hp)
    {
        HPSlider.value = hp;
        PreviewHPSlider.value = hp; // Sin daño anticipado
    }

    public void SetMana(int Mana)
    {
        ManaSlider.value = Mana;
    }

    // FUNCIONES PARA PREVISUALIZACION 
    public void PreviewDamage(int damage)
    {
        PreviewHPSlider.value = Mathf.Max(HPSlider.value - damage, 0);
    }

    public void ApplyDamage(int damage)
    {
        HPSlider.value = Mathf.Max(HPSlider.value - damage, 0);
        PreviewHPSlider.value = HPSlider.value;
    }

    public void ResetPreview()
    {
        PreviewHPSlider.value = HPSlider.value;
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}

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
    
    [Header("Attack Bonus Display")]
    public Text AttackBonusText;      // ✅ NEW: Shows attack bonus from status effects
    
    private Unit currentUnit;         // ✅ NEW: Store reference to current unit

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
        
        // ✅ NEW: Update attack bonus display in real-time
        UpdateAttackBonusDisplay();
    }
    public void SetHUD(Unit unit)
    {
        currentUnit = unit; // ✅ NEW: Store unit reference
        
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
        
        // ✅ NEW: Update attack bonus on HUD setup
        UpdateAttackBonusDisplay();
    }

    public void SetHP(int hp)
    {
        HPSlider.value = hp;
        PreviewHPSlider.value = hp; // Sin da�o anticipado
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
    
    // ✅ NEW: Update attack bonus display based on status effects
    private void UpdateAttackBonusDisplay()
    {
        if (AttackBonusText == null || currentUnit == null) return;
        
        var statusManager = currentUnit.GetComponent<StatusEffectManager>();
        if (statusManager != null)
        {
            float attackBonus = statusManager.CalculateAttackBonusPercent();
            if (attackBonus > 0)
            {
                AttackBonusText.text = $"+{attackBonus} ATK";
                AttackBonusText.gameObject.SetActive(true);
                AttackBonusText.color = Color.green;
            }
            else if (attackBonus < 0)
            {
                AttackBonusText.text = $"{attackBonus} ATK";
                AttackBonusText.gameObject.SetActive(true);
                AttackBonusText.color = Color.red;
            }
            else
            {
                AttackBonusText.gameObject.SetActive(false);
            }
        }
        else
        {
            AttackBonusText.gameObject.SetActive(false);
        }
    }
}

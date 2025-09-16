using UnityEngine;
using UnityEngine.UI;

public class CharacterDetailsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject panel;
    public Text nameText;
    public Text levelText;
    public Slider hpSlider;
    public Text hpValueText;
    public Slider manaSlider;
    public Text manaValueText;
    public Text FueText;
    public Text DesText;
    public Text ConText;
    public Text IntText;

    [Header("Retrato")]
    public Image portraitImage; 

    private Unit currentUnit;
    public BattleSystem battleSystem;

    // void Awake()
    // {
    //     panel.SetActive(false);
    //     UnitSelection.OnUnitSelected += ShowDetails;
    // }

    void OnEnable()
    {
        Debug.Log("[CharacterDetailsUI] OnEnable suscribiendo eventos");
        //UnitSelection.OnUnitSelected += ShowDetails;
        if (battleSystem != null)
        {
            battleSystem.OnTurnStart += ShowDetails;
            Debug.Log("[CharacterDetailsUI] Suscrito a battleSystem.OnTurnStart");
        }
        else
        {
            Debug.LogWarning("[CharacterDetailsUI] battleSystem NO está asignado");
        }
    }


    void OnDisable()
    {
        UnitSelection.OnUnitSelected -= ShowDetails;
        if (battleSystem != null)
            battleSystem.OnTurnStart -= ShowDetails;
    }

    /// <summary>
    /// Llama desde BattleSystem o desde selección de ratón para actualizar UI.
    /// </summary>
    public void ShowDetails(Unit unit)
    {
        bool esEnemigo = unit.CompareTag("Enemy");
        if (esEnemigo)
        {
            panel.SetActive(false);
            return;
        }
        currentUnit = unit;
        portraitImage.sprite = currentUnit.portrait;
        Debug.Log(unit.name);
        // Activar panel
        panel.SetActive(true);
        UpdateAllUI();

    }

    public void UpdateAllUI()
    {
        // Datos básicos
        nameText.text  = currentUnit.Name;
        levelText.text = "Lv " + currentUnit.Level;

        // Vida
        hpSlider.maxValue = currentUnit.maxHP;
        hpSlider.value    = currentUnit.currentHP;
        hpValueText.text  = $"{currentUnit.currentHP}/{currentUnit.maxHP}";

        // Maná
        manaSlider.maxValue = currentUnit.maxMana;
        manaSlider.value    = currentUnit.currentMana;
        manaValueText.text  = $"{currentUnit.currentMana}/{currentUnit.maxMana}";

        // Atributos
        FueText.text = currentUnit.Fue.ToString();
        DesText.text = currentUnit.Des.ToString();
        ConText.text = currentUnit.Con.ToString();
        IntText.text = currentUnit.Int.ToString();
    }

    /// <summary>
    /// Guardar para luego agregarselo a otros botones.
    /// </summary>
    /// 
    // void ChangeFue(int delta)
    // {
    //     currentUnit.Fue = Mathf.Max(0, currentUnit.Fue + delta);
    //     UpdateAllUI();
    // }

    // // Suma o resta puntos de Inteligencia
    // void ChangeDes(int delta)
    // {
    //     currentUnit.Des = Mathf.Max(0, currentUnit.Des + delta);
    //     UpdateAllUI();
    // }

    // void ChangeCon(int delta)
    // {
    //     currentUnit.Con = Mathf.Max(0, currentUnit.Con + delta);
    //     int hpChange    = 5 * delta;
    //     currentUnit.maxHP    = Mathf.Max(1, currentUnit.maxHP + hpChange);
    //     currentUnit.currentHP = Mathf.Clamp(currentUnit.currentHP + hpChange, 0, currentUnit.maxHP);
    //     UpdateAllUI();
    // }

    // // Suma o resta puntos de Inteligencia
    // void ChangeInt(int delta)
    // {
    //     currentUnit.Int = Mathf.Max(0, currentUnit.Int + delta);
    //     int manaChange   = 3 * delta;
    //     currentUnit.maxMana     = Mathf.Max(1, currentUnit.maxMana + manaChange);
    //     currentUnit.currentMana = Mathf.Clamp(currentUnit.currentMana + manaChange, 0, currentUnit.maxMana);
    //     UpdateAllUI();
    // }

    // Opcional: ocultar al hacer clic fuera o presionar ESC
    public void HideDetails()
    {
        panel.SetActive(false);
    }
}

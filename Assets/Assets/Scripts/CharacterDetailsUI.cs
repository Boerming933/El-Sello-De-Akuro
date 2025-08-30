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

    [Header("Botones de stats")]

    public Button FuePlusButton;
    public Button FueMinusButton;
    public Button DesPlusButton;
    public Button DesMinusButton;
    public Button ConPlusButton;
    public Button ConMinusButton;
    public Button IntPlusButton;
    public Button IntMinusButton;

    private Unit currentUnit;
    bool esAliado;

    void Awake()
    {
        panel.SetActive(false);
        UnitSelection.OnUnitSelected += ShowDetails;
    }

    void OnEnable()
    {
        // Suscribirse aquí
        UnitSelection.OnUnitSelected += ShowDetails;
    }

    void OnDestroy()
    {
        UnitSelection.OnUnitSelected -= ShowDetails;
    }

    void Start()
    {
        // Asignar listeners a los botones
        FuePlusButton.onClick.AddListener(() => ChangeFue(+1));
        FueMinusButton.onClick.AddListener(() => ChangeFue(-1));
        DesPlusButton.onClick.AddListener(() => ChangeDes(+1));
        DesMinusButton.onClick.AddListener(() => ChangeDes(-1));
        ConPlusButton.onClick.AddListener(() => ChangeCon(+1));
        ConMinusButton.onClick.AddListener(() => ChangeCon(-1));
        IntPlusButton.onClick.AddListener(() => ChangeInt(+1));
        IntMinusButton.onClick.AddListener(() => ChangeInt(-1));
    }

    void ShowDetails(Unit unit)
    {
        bool esAliado = unit.CompareTag("Aliado");
        if (!esAliado)
        {
            panel.SetActive(false);
            return;
        }
        currentUnit = unit;
        portraitImage.sprite = currentUnit.portrait;
        // Activar panel
        panel.SetActive(true);
        UpdateAllUI();
       
    }

    void UpdateAllUI()
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
    void ChangeFue(int delta)
    {
        currentUnit.Fue = Mathf.Max(0, currentUnit.Fue + delta);
        UpdateAllUI();
    }

    // Suma o resta puntos de Inteligencia
    void ChangeDes(int delta)
    {
        currentUnit.Des = Mathf.Max(0, currentUnit.Des + delta);
        UpdateAllUI();
    }

    void ChangeCon(int delta)
    {
        currentUnit.Con = Mathf.Max(0, currentUnit.Con + delta);
        int hpChange    = 5 * delta;
        currentUnit.maxHP    = Mathf.Max(1, currentUnit.maxHP + hpChange);
        currentUnit.currentHP = Mathf.Clamp(currentUnit.currentHP + hpChange, 0, currentUnit.maxHP);
        UpdateAllUI();
    }

    // Suma o resta puntos de Inteligencia
    void ChangeInt(int delta)
    {
        currentUnit.Int = Mathf.Max(0, currentUnit.Int + delta);
        int manaChange   = 3 * delta;
        currentUnit.maxMana     = Mathf.Max(1, currentUnit.maxMana + manaChange);
        currentUnit.currentMana = Mathf.Clamp(currentUnit.currentMana + manaChange, 0, currentUnit.maxMana);
        UpdateAllUI();
    }

    // Opcional: ocultar al hacer clic fuera o presionar ESC
    public void HideDetails()
    {
        panel.SetActive(false);
    }
}

using UnityEngine;

public enum AreaShape
{
    Circle,
    LineHorizontal,
    LineVertical,
    Cross
}

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public Sprite icon;
    public int damage;
    public int manaCost;
    public float cooldown;

    [Header("Configuración de alcance y área")]
    public int        selectionRange;   // cuántos tiles de alcance para seleccionar el objetivo
    public int        areaSize;         // cuántos tiles abarca el efecto final
    public AreaShape  effectShape;      // forma del área de efecto

    [Header("Modificador de iniciativa")]
    public int initiativeBonus;     // +destreza temporal
    public int initiativeDuration;  // turnos que dura ese bonus

    [TextArea] public string description;
}

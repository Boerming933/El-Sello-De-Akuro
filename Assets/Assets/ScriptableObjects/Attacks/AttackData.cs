using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "Combat/Attack")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public Sprite icon;
    public int damage;
    public int manaCost;
    public float cooldown;
    [TextArea] public string description;
}

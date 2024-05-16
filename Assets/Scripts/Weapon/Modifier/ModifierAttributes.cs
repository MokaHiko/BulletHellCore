using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Modifier", menuName = "Modifier")]
public class ModifierAttributes : ScriptableObject
{
    public new string name;

    [SerializeField]
    private string description;

    public string Description()
    {
        return description;
    }

    public RectTransform modifier_icon;
    public WeaponTypes weapon_types;
    public WeaponModifiers modifiers;

    public WeaponStats stat_multipliers;
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
struct ModifierWeight
{
    [SerializeField]
    public WeaponModifiers modifier;
    [SerializeField]
    public float weight;
};

[Serializable]
struct StatModifierWeight
{
    [SerializeField]
    public WeaponStatModifiers stat_modifier;
    [SerializeField]
    public float weight;
};

[Flags]
public enum WeaponStatModifiers
{
    None,
    FireRate,
    Damage,
    ReloadSpeed,
};

public class RewardUI : MonoBehaviour
{
    [SerializeField]
    WeaponModifiers modifier_flags;

    [SerializeField]
    WeaponStatModifiers stat_modifiers;

    [SerializeField]
    TMP_Text description;

    [SerializeField]
    GameObject rewards_container;

    [SerializeField]
    List<ModifierWeight> weights;

    [SerializeField]
    List<StatModifierWeight> stat_weights;


    private void OnEnable()
    {
        Roll();
    }

    void Roll()
    {
        modifier_flags = WeaponModifiers.None;
        foreach (ModifierWeight weight in weights)
        {
            float roll = UnityEngine.Random.Range(0, 1.0f);
            if (weight.weight >= roll)
            {
                modifier_flags |= weight.modifier;
                break;
            }
        }

        stat_modifiers = WeaponStatModifiers.None;
        foreach (StatModifierWeight weight in stat_weights)
        {
            float roll = UnityEngine.Random.Range(0, 1.0f);
            if (weight.weight >= roll)
            {
                stat_modifiers |= weight.stat_modifier;
            }
        }

        description.text = "";
        switch (modifier_flags)
        {
        case (WeaponModifiers.Bounce):
        {
            description.text = "Applies +1 bounce";
        }break;
        case(WeaponModifiers.Echo):
        {
            description.text = "Applies +1 echo";
        }break;
            case (WeaponModifiers.None):
            default:
                break;
        }

        if ((stat_modifiers & WeaponStatModifiers.FireRate) == WeaponStatModifiers.FireRate)
        {
            description.text += "\n Increased fire rate!";
        }

        if ((stat_modifiers & WeaponStatModifiers.Damage) == WeaponStatModifiers.Damage)
        {
            description.text += "\n Increased base damage!";
        }

        if ((stat_modifiers & WeaponStatModifiers.ReloadSpeed) == WeaponStatModifiers.ReloadSpeed)
        {
            description.text += "\n Decreased reload speed!";
        }
    }

    public void Upgrade()
    {
        PlayerController player = GameManager.Instance.GetPlayer();
        if (player)
        {
            Weapon weapon = player.GetComponent<Unit>().EquipedWeapon;
            Modifier modifier = weapon.GetComponent<Modifier>();

            switch (modifier_flags)
            {
            case (WeaponModifiers.Bounce):
            {
                modifier.ApplyModifier(WeaponModifiers.Bounce);
                modifier.bounces++;
                description.text = "Applies +1 bounce";
            }break;
            case(WeaponModifiers.Echo):
            {
                modifier.ApplyModifier(WeaponModifiers.Echo);
                modifier.echo_count = 1;
                description.text = "Applies +1 echo"; // Echo first, hit only
            }break;
                case (WeaponModifiers.None):
                default:
                    break;
            }

            if ((stat_modifiers & WeaponStatModifiers.FireRate) == WeaponStatModifiers.FireRate)
            {
                description.text += "\n Increased fire rate!";
                weapon.attack_speed *= 1.15f;
            }

            if ((stat_modifiers & WeaponStatModifiers.Damage) == WeaponStatModifiers.Damage)
            {
                description.text += "\n Increased base damage!";
                weapon.base_damage *= 1.15f;
            }

            if ((stat_modifiers & WeaponStatModifiers.ReloadSpeed) == WeaponStatModifiers.ReloadSpeed)
            {
                description.text += "\n Decreased reload speed!";
                weapon.reload_time *= 1 - 0.15f;
            }
        }

        rewards_container.SetActive(false);
    }
}

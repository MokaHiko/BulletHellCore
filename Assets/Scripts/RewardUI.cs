using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
struct ModifierWeight
{
    public WeaponModifiers modifier;
    public float weight;
};

public class RewardUI : MonoBehaviour
{
    [SerializeField]
    WeaponModifiers modifier_flags;

    [SerializeField]
    TMP_Text description;

    [SerializeField]
    MenuManager in_game_menu;

    [SerializeField]
    List<ModifierWeight> weights;

    [SerializeField]
    List<ModifierAttributes> possible_modifiers;

    [SerializeField]
    ModifierAttributes current_modifier;

    [SerializeField]
    RectTransform modifier_icon;

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

        //if ((stat_modifiers & WeaponStatModifiers.FireRate) == WeaponStatModifiers.FireRate)
        //{
        //    description.text += "\n Increased fire rate!";
        //}

        //if ((stat_modifiers & WeaponStatModifiers.Damage) == WeaponStatModifiers.Damage)
        //{
        //    description.text += "\n Increased base damage!";
        //}

        //if ((stat_modifiers & WeaponStatModifiers.ReloadSpeed) == WeaponStatModifiers.ReloadSpeed)
        //{
        //    description.text += "\n Decreased reload speed!";
        //}

        current_modifier = possible_modifiers[UnityEngine.Random.Range(0, possible_modifiers.Count)];

        if(modifier_icon != null)
        {
            Destroy(modifier_icon.gameObject);
        }

        Instantiate(current_modifier.modifier_icon, transform);
        description.text = current_modifier.Description();
    }

    public void Upgrade()
    {
        PlayerController player = GameManager.Instance.GetPlayer();
        Merc merc = player.party_slots[UnityEngine.Random.Range(0, player.party_slots.Count)].merc;

        if (player && merc)
        {
            //Weapon weapon = player.GetComponent<Unit>().EquipedWeapon;
            //Modifier modifier = merc.GetComponent<Unit>().e.GetComponent<Modifier>();
            Weapon weapon = merc.GetComponent<Unit>().EquipedWeapon;

            //switch (modifier_flags)
            //{
            //case (WeaponModifiers.Bounce):
            //{
            //    modifier.ApplyModifier(WeaponModifiers.Bounce);
            //    modifier.bounces++;
            //    description.text = "Applies +1 bounce";
            //}break;
            //case(WeaponModifiers.Echo):
            //{
            //    modifier.ApplyModifier(WeaponModifiers.Echo);
            //    modifier.echo_count = 1;
            //    description.text = "Applies +1 echo"; // Echo first, hit only
            //}break;
            //    case (WeaponModifiers.None):
            //    default:
            //        break;
            //}

            //if ((stat_modifiers & WeaponStatModifiers.FireRate) == WeaponStatModifiers.FireRate)
            //{
            //    description.text += "\n Increased fire rate!";
            //    //weapon.attack_speed *= 1.15f;
            //}

            //if ((stat_modifiers & WeaponStatModifiers.Damage) == WeaponStatModifiers.Damage)
            //{
            //    description.text += "\n Increased base damage!";
            //    //weapon.base_damage *= 1.15f;
            //}

            //if ((stat_modifiers & WeaponStatModifiers.ReloadSpeed) == WeaponStatModifiers.ReloadSpeed)
            //{
            //    description.text += "\n Decreased reload speed!";
            //    //weapon.reload_time *= 1 - 0.15f;
            //}

            weapon.AddModifier(current_modifier);
        }

        in_game_menu.DeactivateMenu("RewardsMenu");
        var menu = in_game_menu.FindMenu("PartyLayoutMenu");
        in_game_menu.ActivateMenu(menu);
    }
}

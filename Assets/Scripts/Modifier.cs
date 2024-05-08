using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Flags]
public enum WeaponModifiers
{
    None = 0,
    Bounce = 1 << 0, 
    Echo = 1 << 1, 
    Sap = 1 << 2, 
    LockedOn = 1 << 3,  

    EchoNoCost = 1 << 3, 
};

[RequireComponent(typeof(Weapon))]
public class Modifier : MonoBehaviour
{
    // TODO: Make WeaponModifierEvent Class
    [Header("OnFire")]
    [SerializeField] public List<Ability> on_fire_abilities;
    [SerializeField] public WeaponModifiers modifiers;

    [Header("Echo")]
    [SerializeField] public float echo_delay = 1.0f;
    [SerializeField] public int echo_count = 0;

    [Header("Richochet")]
    [SerializeField] public float bounce_multiplier = 0.5f;
    [SerializeField] public int bounces = 0;

    // On Echo
    // On Bounce

    // Start is called before the first frame update
    void Start()
    {
        OnEquip();
    }

    public void ApplyModifier(WeaponModifiers modifier_flags)
    {
        if (CheckModifier(modifier_flags))
        {
            return;
        }

        modifiers |= modifier_flags;

        if(CheckModifier(WeaponModifiers.Echo))
        {
            m_weapon.on_fire += Echo;
        }

        if(CheckModifier(WeaponModifiers.Bounce))
        {
            int n_bounces = bounces;
            m_weapon.on_hit += (Vector3 point, Vector3 dir, Vector3 normal) =>
            {
                if (n_bounces <= 0)
                {
                    n_bounces = bounces;
                    return;
                }

                Bounce(point, dir, normal);
                n_bounces--;
            };
        }

        if(CheckModifier(WeaponModifiers.Sap))
        {
            m_weapon.on_hit += (Vector3 point, Vector3 dir, Vector3 normal) =>
            {
            };
        }
    }
    public bool CheckModifier(WeaponModifiers modifier_flags){ return (modifiers & modifier_flags) == modifier_flags;}
    public void RemoveModifier(WeaponModifiers modifier_flags) { modifiers &= ~modifier_flags; }

    void OnEquip()
    {
        m_weapon = GetComponent<Weapon>();

        if(CheckModifier(WeaponModifiers.Echo))
        {
            m_weapon.on_fire += Echo;
        }

        if(CheckModifier(WeaponModifiers.Bounce))
        {
            m_weapon.on_hit += (Vector3 point, Vector3 dir, Vector3 normal) =>
            {
                Bounce(point, dir, normal);
            };
        }

        if(CheckModifier(WeaponModifiers.Sap))
        {
            m_weapon.on_hit += (Vector3 point, Vector3 dir, Vector3 normal) =>
            {
            };
        }
    }

    void OnUnequip()
    {
        // TODO: Unsub from events
        if(CheckModifier(WeaponModifiers.Echo))
        {
            m_weapon.on_fire -= Echo;
        }
    }

    void Echo(Vector3 target_position)
    {
        StartCoroutine(EchoEffect(target_position, echo_count, echo_delay));
    }
    private IEnumerator EchoEffect(Vector3 target_position, int echo_count, float delay_time = 0, Action callback = null)
    {
        yield return new WaitForSeconds(delay_time);

        echo_count--;
        if (m_weapon != null)
        {
            m_weapon.AttackImpl(transform.position, target_position);
        }

        callback?.Invoke();

        if (echo_count > 0)
        {
            StartCoroutine(EchoEffect(target_position, echo_count, delay_time));
        }
    }
    private void Bounce(Vector3 point, Vector3 dir, Vector3 normal)
    {
        float start_damage = m_weapon.base_damage;
        m_weapon.base_damage *= bounce_multiplier;

        dir.y = 0;
        dir.Normalize();
        m_weapon.AttackImpl(point, Vector3.Reflect(dir, normal).normalized * m_weapon.range);
        m_weapon.base_damage = start_damage;
    }

    IEnumerator Delay(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }
    void WeaponAttack(Vector3 world_position)
    {
    }

    private Weapon m_weapon;
}

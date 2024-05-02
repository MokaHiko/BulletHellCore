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
};


[RequireComponent(typeof(Weapon))]
public class Modifier : MonoBehaviour
{
    // TODO: Make WeaponModifierEvent Class
    [Header("OnFire")]
    [SerializeField] List<Ability> on_fire_abilities;
    [SerializeField] WeaponModifiers modifiers;

    [Header("Echo")]
    [SerializeField] float echo_delay = 1.0f;

    [Header("Richochet")]
    [SerializeField] float bounce_multiplier = 0.5f;
    [SerializeField] int bounces = 1;
 
    //List<Ability> on_fire_abilities;

    // Start is called before the first frame update
    void Start()
    {
        OnEquip();
    }
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
                if(bounces > 0) 
                { 
                    Bounce(point, dir, normal);
                }
                else
                {
                    bounces = 1;
                }
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
        StartCoroutine(Delay(echo_delay, () =>
        {
            if (m_weapon != null)
            {
                m_weapon.AttackImpl(transform.position, target_position);
            }
        }));
    }
    private void Bounce(Vector3 point, Vector3 dir, Vector3 normal)
    {
        // Decrement bounce
        bounces--;
        m_weapon.AttackImpl(point, Vector3.Reflect(dir, normal).normalized * m_weapon.range, false);
    }

    IEnumerator Delay(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }
    void WeaponAttack(Vector3 world_position)
    {
    }
    public bool CheckModifier(WeaponModifiers modifier_flags){ return (modifiers & modifier_flags) == modifier_flags;}

    private Weapon m_weapon;
}

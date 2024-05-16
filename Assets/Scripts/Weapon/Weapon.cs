using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void WeaponFireCallback(Vector3 world_position);
public delegate void WeaponHitCallback(Vector3 point, Vector3 dir, Vector3 normal = new Vector3());
public delegate void WeaponReloadCallback();


[Flags]
public enum WeaponTypes
{
    None = 0,
    ChargeGun = 1 << 0,
    Flamethrower = 1 << 1,
    Shield = 1 << 2,
};

[Serializable]
public class WeaponStats
{
    public float base_damage = 1;
    public float attack_speed = 1f;
    public float scan_radius = 1.0f;
    public float range = 1.0f;
    public int max_bullets = 1;
    public float reload_time = 1.0f;
    public float hold_threshold = 1.0f;
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField]
    public WeaponResource resource;
    [SerializeField]
    public List<ModifierAttributes> modifiers;

    public LayerMask damageable_layers;

    // ~ Handles
    public Unit owner;

    // ~ Callbacks
    public WeaponFireCallback on_fire;

    public WeaponHitCallback on_hit; 

    public WeaponReloadCallback on_reload; 

    // ~ Getters
    public WeaponStats Stats { get { return m_modified_stats; } }

    public void AddModifier(ModifierAttributes modifier)
    {
        modifiers.Add(modifier);

        // Recalculate stats
        CalculateModifierStats();
    }

    protected void Shake(float intensity, float time)
    {
        GameManager.Instance.RequestShake(intensity, time);
    }

    void Awake()
    {
        // Copy base stats
        m_bullets = resource.base_stats.max_bullets;

        CalculateModifierStats();
        OnEquip();
    }

    public void CalculateModifierStats()
    {
        // Copy base stats
        m_modified_stats = new WeaponStats();
        m_modified_stats.base_damage = resource.base_stats.base_damage;
        m_modified_stats.attack_speed = resource.base_stats.attack_speed;
        m_modified_stats.scan_radius = resource.base_stats.scan_radius;
        m_modified_stats.reload_time = resource.base_stats.reload_time;
        m_modified_stats.hold_threshold = resource.base_stats.hold_threshold;

        m_modified_stats.max_bullets = resource.base_stats.max_bullets;
        m_modified_stats.range = resource.base_stats.range;

        foreach (ModifierAttributes attr in modifiers)
        {
            m_modified_stats.base_damage  *= attr.stat_multipliers.base_damage;
            m_modified_stats.attack_speed *= attr.stat_multipliers.attack_speed;
            m_modified_stats.scan_radius *= attr.stat_multipliers.scan_radius;
            m_modified_stats.reload_time *= attr.stat_multipliers.reload_time;
            m_modified_stats.hold_threshold *= attr.stat_multipliers.hold_threshold;

            // bullets are additive
            m_modified_stats.max_bullets += attr.stat_multipliers.max_bullets;
        }
    }

    public void Attack(Vector3 target_position, bool alt_attack = false)
    {
        if (m_bullets < 0)
        {
            Reload();
            return;
        }

        if(alt_attack)
        {
            AltAttackImpl(transform.position, target_position);
        }
        else
        {
            if(m_time_since_last_fire <= (1.0f / Stats.attack_speed))
            {
                return;
            }
            m_time_since_last_fire = 0.0f;
            m_bullets--;
            AttackImpl(transform.position, target_position);
        }
        on_fire?.Invoke(target_position);
    }

    public virtual void AttackImpl(Vector3 fire_point, Vector3 target_position) { }
    public virtual void AltAttackImpl(Vector3 fire_point, Vector3 target_position) { }

    public void Reload()
    {
        // Check if already reloading
        if (m_reload_coroutine == null)
        {
            on_reload?.Invoke();
            m_reload_coroutine = StartCoroutine(ReloadEffect());
        }
    }

    public IEnumerator ReloadEffect()
    {
        yield return new WaitForSeconds(Stats.reload_time);
        m_bullets = Stats.max_bullets;
        m_reload_coroutine = null;
    }

    public void Update()
    {
        m_time_since_last_fire += Time.deltaTime;
    }

    void OnEquip()
    {
        owner = GetComponentInParent<Unit>();
    }
    
    // ~ Weapon Common
    protected float m_time_since_last_fire = 0.0f;
    [SerializeField]
    protected int m_bullets = 0;
    [SerializeField]
    WeaponStats m_modified_stats;

    Coroutine m_reload_coroutine = null;
}


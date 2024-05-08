using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

[Flags]
public enum UnitStateFlags
{
    None = 0,

    Moving = 1 << 0,
    ManagedMovement = 1 << 1,

    TakingDamage = 1 << 2,

    Dead = 1 << 3,
};

[RequireComponent(typeof(Rigidbody))]
public class Unit : IDamageable
{
    [Header("Unit State")]
    public float energy = 0.0f;
    public float fossil = 0.0f;
    public float nuclear = 0.0f;
    public float solar = 0.0f;
    public float energy_gain_rate = 1.0f;
    public float movement_speed = 0.0f;
    public float agility = 0.0f;

    [Header("Combat")]
    [SerializeField]
    public List<Weapon> weapons;

    [Header("Unit Handles")]
    public Animator animator;

    // Unit UI
    public PropertyBar health_bar;
    public PropertyBar energy_bar;

    // Status Effect Handles
    public ParticleSystem short_circuit_particles;

    // Getters
    public UnitStats BaseStats {get { return m_base_stats; }}
    public Rigidbody GetRigidbody {get {return m_rigidbody;}}
    public Weapon EquipedWeapon {get {return weapons.Count > 0 ? weapons[0] : null;}}

    // State Flags Helpers
    public bool CheckState(UnitStateFlags state_flags){ return (m_state & state_flags) == state_flags;}
    public void ApplyState(UnitStateFlags state_flags){ m_state |= state_flags;}
    public void RemoveState(UnitStateFlags state_flags) { m_state &= ~state_flags; }

    // Returns whether or not there was enough energy
    public bool SpendEnergy(float amount)
    {
        // Check already negative or short circuited
        if (energy < 0.0f || CheckStatus(StatusEffect.ShortCircuit))
        {
            return false;
        }

        // Barrow but short circuit
        if (energy - amount < 0.0f)
        {
            // Reset if already short circuited
            // if (m_short_circuit_routine != null)
            // {
            //     StopCoroutine(m_short_circuit_routine);
            //     m_short_circuit_routine = null;
            // }

            // ApplyStatus(StatusEffect.ShortCircuit);
            // m_short_circuit_routine = StartCoroutine(ShortCircuitEffect());
            return true;
        }

        energy -= amount;
        return true;
    }
    public void UseAbility(AbilityType type, bool burst = false, Vector3 direction = new Vector3())
    {
        int type_index = (int)type;
        if (type_index < abilities.Count)
        {
            abilities[type_index].UseWithCost(burst, direction);
            return;
        }

        //Debug.Log("Unit has no ability at index: " + type_index);
    }
    public void IncrementAblilityStack(AbilityType type)
    {
        int type_index = (int)type;
        if (type_index < abilities.Count)
        {
            abilities[type_index].IncrementStack();
            return;
        }

        //Debug.Log("Unit has no ability at index: " + type_index);
    }
    
    private void Awake()
    {
        // Handles
        m_rigidbody = GetComponent<Rigidbody>();

        // Damageable Callbacks
        damaged_callback += (float damage) =>
        {
            StartCoroutine(UnitDamagedEffect());
        };

        // Effect asserts
        Debug.Assert(health_bar != null);;
        Debug.Assert(energy_bar != null);;

        // Get Renderable Components
        if(TryGetComponent<Animator>(out Animator anim))
        {
            animator = anim;
        }
        else
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Defaults Values
        m_state = UnitStateFlags.None;

        m_health = m_base_stats.health;
        energy = m_base_stats.max_energy * 0.75f;
        energy_gain_rate = m_base_stats.energy_gain_rate;
        movement_speed = m_base_stats.movement_speed;
        agility = m_base_stats.agility;
        damage_multiplier = m_base_stats.damage_multiplier;

        health_bar.SetValue(m_health, m_base_stats.health);
        energy_bar.SetValue(energy, m_base_stats.max_energy);

        // Get Ability Handles
        foreach(Ability ability in GetComponents<Ability>())
        {
            abilities.Add(ability);
        }
    }
    void Update() 
    {
        energy = Mathf.Clamp(energy + (energy_gain_rate * Time.deltaTime), 0, m_base_stats.max_energy);

        // Set UI
        energy_bar.SetValue(energy, m_base_stats.max_energy);
        health_bar.SetValue(m_health, m_base_stats.health);

        if (animator)
        {
            animator.SetFloat("normalized_ms", GetComponent<Rigidbody>().velocity.magnitude / m_base_stats.movement_speed);
        }
    }

    private IEnumerator UnitDamagedEffect()
    {
        // Slow movement
        ApplyState(UnitStateFlags.TakingDamage);
        movement_speed = m_base_stats.movement_speed * 0.75f;
        agility = m_base_stats.agility * 0.75f;

        yield return new WaitForSeconds(0.15f);

        RemoveState(UnitStateFlags.TakingDamage);
        agility = m_base_stats.agility;
        movement_speed = m_base_stats.movement_speed;
    }

    // ~ Stats
    [SerializeField]
    private UnitStats m_base_stats;

    // ~ State
    [SerializeField]
    private UnitStateFlags m_state = UnitStateFlags.None;

    // ~ Abilities
    [SerializeField]
    public List<Ability> abilities;

    bool m_burst = false;

    // ~ Handles
    public Rigidbody m_rigidbody;
 }

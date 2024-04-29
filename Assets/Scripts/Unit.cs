using System;
using System.Collections;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

[Flags]
public enum UnitState
{
    None = 0,

    Moving = 1 << 0,
    ManagedMovement = 1 << 1,

    TakingDamage = 1 << 2,

    Dead = 1 << 8,
};

[Flags]
public enum StatusEffect
{
    None = 0,
    Burning = 1 << 0, 
    ShortCircuit = 1 << 1,
    Armored = 1 << 2,
};

[System.Serializable]
public struct UnitStats
{
    public float health;
    public float max_energy;
    public float energy_gain_rate;
    public float movement_speed;
    public float agility;
    public float damage_multiplier;
};

// Called whenever the unit is damaged
public delegate void UnitDamagedCallback(float damage);

// Called when a status is applied to the unity
public delegate void UnitStatusCallback(StatusEffect status_effects);

// Called before the unit dies
public delegate void UnitDeathCallback();

public class Unit : MonoBehaviour
{
    [SerializeField]
    public float health = 0.0f;

    [SerializeField]
    public float energy = 0.0f;

    [SerializeField]
    public float energy_gain_rate = 1.0f;

    [SerializeField]
    public float movement_speed = 0.0f;

    [SerializeField]
    public float agility = 0.0f;

    [SerializeField]
    public float damage_multiplier = 1.0f;

    // Callbacks
    public UnitDeathCallback death_callback;
    public UnitDamagedCallback damaged_callback;
    public UnitStatusCallback status_callback;

    // Status Effect Handles
    public ParticleSystem short_circuit_particles;

    // Getters
    public UnitStats BaseStats() { return m_base_stats; }

    // State Flags Helpers
    public bool CheckState(UnitState state_flags){ return (m_state & state_flags) == state_flags;}
    public void ApplyState(UnitState state_flags){ m_state |= state_flags;}
    public void RemoveState(UnitState state_flags) { m_state &= ~state_flags; }

    // Status Flags Helpers
    public bool CheckStatus(StatusEffect status_flags){ return (m_status_effect & status_flags) == status_flags;}
    public void ApplyStatus(StatusEffect status_flags) 
    {
        m_status_effect |= status_flags;
        status_callback?.Invoke(status_flags);
    }
    public void RemoveStatus(StatusEffect status_flags) { m_status_effect &= ~status_flags; }

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
            if (m_short_circuit_routine != null)
            {
                StopCoroutine(m_short_circuit_routine);
                m_short_circuit_routine = null;
            }
            ApplyStatus(StatusEffect.ShortCircuit);
            m_short_circuit_routine = StartCoroutine(ShortCircuitEffect());
            return true;
        }

        energy -= amount;
        return true;
    }
    public void TakeDamage(float damage, StatusEffect status_effect = StatusEffect.None)
    {
        ApplyStatus(status_effect);
        
        if(m_damage_effect_routine == null) 
        {
            m_damage_effect_routine = StartCoroutine(nameof(DefaultDamageEffect));
        }
        m_last_damage = damage * damage_multiplier;
        health -= damage * damage_multiplier;

        damaged_callback?.Invoke(m_last_damage);

        if (health < 0.0f)
        {
            // Invoke callbacks
            death_callback?.Invoke();

            // Clean up
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_start_color = m_renderer.material.color;

        // Defaults Values
        m_state = UnitState.None;
        m_status_effect = StatusEffect.None;

        health = m_base_stats.health;
        energy = m_base_stats.max_energy;
        energy_gain_rate = m_base_stats.energy_gain_rate;
        movement_speed = m_base_stats.movement_speed;
        agility = m_base_stats.agility;
        damage_multiplier = m_base_stats.damage_multiplier;
    }

    void Update() 
    {
        energy = Mathf.Clamp(energy + (energy_gain_rate * Time.deltaTime), 0, m_base_stats.max_energy);
        if(m_status_effect == StatusEffect.None) 
        { 
            return;
        }

        switch (m_status_effect)
        {
            case StatusEffect.ShortCircuit:
            {
            }break;
            case StatusEffect.Burning:
            {
            }break;
            default:
            break;
        }
    }

    private IEnumerator ShortCircuitEffect()
    {
        float start_speed = movement_speed;
        float start_aglity = agility;
        movement_speed *= 0.25f;
        agility *= 0.25f;

        short_circuit_particles.Play();

        yield return new WaitForSeconds(3.0f);

        short_circuit_particles.Stop();
        movement_speed = start_speed;
        agility = start_aglity;

        RemoveStatus(StatusEffect.ShortCircuit);

        m_short_circuit_routine = null;
    }

    private IEnumerator DefaultDamageEffect()
    {
        // TODO: Change to shader wipe
        ApplyState(UnitState.TakingDamage);
        m_renderer.material.color = Color.white;

        yield return new WaitForSeconds(0.1f);

        m_renderer.material.color = m_start_color;
        RemoveState(UnitState.TakingDamage);

        m_damage_effect_routine = null;
    }

    // Stats
    [SerializeField]
    private UnitStats m_base_stats;

    // ~ Effects

    // Damaged
    private Color m_start_color;
    private Coroutine m_damage_effect_routine;

    // ~ State
    [SerializeField]
    private UnitState m_state = UnitState.None;

    // ~ Status
    [SerializeField]
    private StatusEffect m_status_effect = StatusEffect.None;

    private float m_last_damage = 0;
    private Coroutine m_short_circuit_routine = null;

    // ~ Handles
    private Renderer m_renderer;
 }

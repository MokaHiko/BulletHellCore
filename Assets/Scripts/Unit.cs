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

public delegate void UnitDeathCallback();
public delegate void UnitDamagedCallback(float damage);

public class Unit : MonoBehaviour
{
    const float min_energy = 0.0f;
    const float max_energy = 100.0f;

    [SerializeField]
    public float health = 0.0f;

    [SerializeField]
    public float energy = max_energy;

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

    // Status Effect Handles
    public ParticleSystem short_circuit_particles;

    // State Flags Helpers
    public bool CheckState(UnitState state_flags){ return (m_state & state_flags) == state_flags;}
    public void ApplyState(UnitState state_flags){ m_state |= state_flags;}
    public void RemoveState(UnitState state_flags) { m_state &= ~state_flags; }

    // Status Flags Helpers
    public bool CheckStatus(StatusEffect status_flags){ return (m_status_effect & status_flags) == status_flags;}
    public void ApplyStatus(StatusEffect status_flags){ m_status_effect |= status_flags;}
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
            if (m_short_circuit_routine == null)
            {
                m_short_circuit_routine = StartCoroutine(ShortCircuitEffect());
                return true;
            }
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

        // Defaults
        m_state = UnitState.None;
        m_status_effect = StatusEffect.None;
    }

    void Update() 
    {
        energy = Mathf.Clamp(energy + (energy_gain_rate * Time.deltaTime), min_energy, max_energy);
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
        ApplyStatus(StatusEffect.ShortCircuit);

        float start_speed = movement_speed;
        movement_speed *= 0.25f;

        short_circuit_particles.Play();

        yield return new WaitForSeconds(1.5f);

        short_circuit_particles.Stop();
        movement_speed = start_speed;
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

    // ~ Effects

    // Damaged
    private Color m_start_color;
    private Coroutine m_damage_effect_routine;

    // ~ State

    // ~ Status
    [SerializeField]
    private UnitState m_state = UnitState.None;

    private StatusEffect m_status_effect = StatusEffect.None;
    private float m_last_damage = 0;
    private Coroutine m_short_circuit_routine = null;

    // ~ Handles
    private Renderer m_renderer;
 }

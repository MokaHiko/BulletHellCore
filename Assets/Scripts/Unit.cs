using System;
using System.Collections;
using System.Collections.Generic;
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
    Corrosion = 1 << 2,
    Armored = 1 << 3,
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
    public float fossil = 0.0f;

    [SerializeField]
    public float nuclear = 0.0f;

    [SerializeField]
    public float solar = 0.0f;

    [SerializeField]
    public float energy_gain_rate = 1.0f;

    [SerializeField]
    public float movement_speed = 0.0f;

    [SerializeField]
    public float agility = 0.0f;

    [SerializeField]
    public float damage_multiplier = 1.0f;

    // ~ Handles
    public Renderer unit_renderer;
    public Animator animator;

    // Unit UI
    public PropertyBar health_bar;
    public PropertyBar energy_bar;

    // Callbacks
    public UnitDeathCallback death_callback;
    public UnitDamagedCallback damaged_callback;
    public UnitStatusCallback status_callback;

    // Status Effect Handles
    public ParticleSystem short_circuit_particles;
    public GameObject floating_text;

    // Getters
    public UnitStats BaseStats() { return m_base_stats; }
    public float LastDamage() { return m_last_damage; }

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

        if ((status_flags & StatusEffect.Corrosion) == StatusEffect.Corrosion)
        {
            if (m_corossion_routine != null)
            {
                StopCoroutine(m_corossion_routine);
            }
            m_corossion_routine = StartCoroutine(CorossionEffect());
        }
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
    public void TakeDamage(float damage, StatusEffect damage_type = StatusEffect.None, float crit_roll = 0.0f)
    {
        if(m_damage_effect_routine == null) 
        {
            m_damage_effect_routine = StartCoroutine(nameof(DefaultDamageEffect));
        }
        else
        {
            StopCoroutine(m_damage_effect_routine);
            m_damage_effect_routine = StartCoroutine(nameof(DefaultDamageEffect));
        }

        // Check for crit
        Color damage_color = Color.white;
        if (damage_type == StatusEffect.Corrosion)
        {
            damage_color = Color.green;
        }
        else if (crit_roll > 0.95f)
        {
            damage *= UnityEngine.Random.Range(2.0f, 3.0f);
            damage_color = Color.red;
        }
        else if(crit_roll > 0.85f)
        {
            damage *= UnityEngine.Random.Range(1.5f, 2.0f);
            damage_color = Color.yellow;
        }
        else
        {
            damage *= UnityEngine.Random.Range(0.8f, 1.2f);
        }
        damage = (int)damage;

        m_last_damage = damage * damage_multiplier;
        health -= damage * damage_multiplier;
        health_bar.SetValue(health, m_base_stats.health);

        damaged_callback?.Invoke(m_last_damage);

        // Effect
        Vector3 start_pos = transform.position + new Vector3(0.0f, 10.0f, 0.0f);
        var damage_number = Instantiate(floating_text, start_pos, Quaternion.identity, transform);
        TextMesh text_mesh = damage_number.GetComponent<TextMesh>();
        text_mesh.text = damage.ToString();
        text_mesh.color = damage_color;

        if (health <= 0.0f)
        {
            // Invoke callbacks
            death_callback?.Invoke();
            death_callback = null;

            if (m_death_effect)
            {
                ParticleSystem death_effect = Instantiate(m_death_effect, transform.position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
                Destroy(death_effect.gameObject, death_effect.main.duration);

                // De parent effects
                damage_number.transform.SetParent(death_effect.transform);
            }

            // Clean up
            Destroy(gameObject);
        }
    }
    public void UseAbility(AbilityType type, bool burst = false, Vector3 direction = new Vector3())
    {
        int type_index = (int)type;
        if (type_index < abilities.Count)
        {
            abilities[type_index].UseWithCost(burst, direction);
            return;
        }

        Debug.Log("Unit has no ability at index: " + type_index);
    }
    public void IncrementAblilityStack(AbilityType type)
    {
        int type_index = (int)type;
        if (type_index < abilities.Count)
        {
            abilities[type_index].IncrementStack();
            return;
        }

        Debug.Log("Unit has no ability at index: " + type_index);
    }
    
    private void Awake()
    {
        // Effect asserts
        Debug.Assert(m_damaged_material != null);
        Debug.Assert(floating_text != null);;
        Debug.Assert(health_bar != null);;
        Debug.Assert(energy_bar != null);;

        // Get Renderable Components
        if(TryGetComponent<Renderer>(out Renderer r))
        {
            unit_renderer = r; 
        }
        else
        {
            unit_renderer = GetComponentInChildren<Renderer>();
        }

        m_start_material = unit_renderer.material;
        m_start_color = unit_renderer.material.color;

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
        m_state = UnitState.None;
        m_status_effect = StatusEffect.None;

        health = m_base_stats.health;
        energy = m_base_stats.max_energy * 0.75f;
        energy_gain_rate = m_base_stats.energy_gain_rate;
        movement_speed = m_base_stats.movement_speed;
        agility = m_base_stats.agility;
        damage_multiplier = m_base_stats.damage_multiplier;

        health_bar.SetValue(health, m_base_stats.health);
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
        health_bar.SetValue(health, m_base_stats.health);

        if (animator)
        {
            animator.SetFloat("normalized_ms", GetComponent<Rigidbody>().velocity.magnitude / m_base_stats.movement_speed);
        }

        if(m_status_effect == StatusEffect.None) 
        { 
            return;
        }

        switch (m_status_effect)
        {
            case StatusEffect.ShortCircuit:
            {
            }break;
            case StatusEffect.Corrosion:
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

        movement_speed *= 0.5f;

        short_circuit_particles.Play();

        yield return new WaitForSeconds(2.0f);

        short_circuit_particles.Stop();
        movement_speed = start_speed;

        RemoveStatus(StatusEffect.ShortCircuit);

        m_short_circuit_routine = null;
    }
    private IEnumerator CorossionEffect()
    {
        float time = 0.0f;

        float interval_timer = 0.0f;
        while (time < 10.0f)
        {
            time += Time.deltaTime;
            interval_timer += Time.deltaTime;

            if (interval_timer > 0.5f)
            {
                TakeDamage(2.5f, StatusEffect.Corrosion);
                interval_timer = 0.0f;
            }

            yield return null;
        }

        RemoveStatus(StatusEffect.Corrosion);
        m_short_circuit_routine = null;
    }
    private IEnumerator DefaultDamageEffect()
    {
        // Flash white
        ApplyState(UnitState.TakingDamage);
        unit_renderer.material = m_damaged_material;

        yield return new WaitForSeconds(0.1f);

        unit_renderer.material = m_start_material;
        RemoveState(UnitState.TakingDamage);

        // Stop movement
        movement_speed = 0.0f;
        agility = 0.0f;

        yield return new WaitForSeconds(0.15f);

        movement_speed = m_base_stats.movement_speed;
        agility = m_base_stats.agility;

        m_damage_effect_routine = null;
    }

    // Stats
    [SerializeField]
    private UnitStats m_base_stats;

    // ~ Effects
    private Material m_start_material;

    // Damaged
    [SerializeField]
    private Material m_damaged_material;
    [SerializeField]
    private ParticleSystem m_death_effect;

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
    private Coroutine m_corossion_routine = null;

    // ~ Abilities
    [SerializeField]
    public List<Ability> abilities;
 }

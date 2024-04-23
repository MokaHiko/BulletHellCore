using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public enum StatusEffect
{
    Clean,
    Burning, 
    Shocked, 

    Max, 
};

public delegate void UnitDeathCallback();

public class Unit : MonoBehaviour
{
    [SerializeField]
    public float health = 0.0f;

    [SerializeField]
    public float movement_speed = 0.0f;

    [SerializeField]
    public float agility = 0.0f;

    [SerializeField]
    public float damage_multiplier = 1.0f;

    // Callbacks
    public UnitDeathCallback death_callback;

    private void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_start_color = m_renderer.material.color;
    }

    public void TakeDamage(float damage, StatusEffect status_effect = StatusEffect.Clean)
    {
        // Apply status effect 
        switch (status_effect)
        {
            case StatusEffect.Burning:
            {
            }
            break;
            case StatusEffect.Max:
            default:
            {
            }break;
        }
        
        if(m_damage_effect_routine == null) 
        { 
            m_damage_effect_routine = StartCoroutine(nameof(DefaultDamageEffect));
        }

        // Do damage
        health -= damage * damage_multiplier;
        if (health < 0.0f)
        {
            death_callback.Invoke();
            Destroy(gameObject);
        }
    }

    void Update() 
    { 
        if(m_status_effect == StatusEffect.Clean) 
        { 
            return; 
        }
    }

    void ApplyStatus(StatusEffect status)
    {
        m_status_effect |= status;
    }

    private IEnumerator DefaultDamageEffect()
    {
        // TODO: Change to shader wipe
        m_renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        m_renderer.material.color = m_start_color;
        m_damage_effect_routine = null;
    }

    // ~ Effects

    // Damaged
    private Color m_start_color;
    private Coroutine m_damage_effect_routine;

    // ~ State

    // Status
    private StatusEffect m_status_effect = StatusEffect.Clean;

    // ~ Handles
    private Renderer m_renderer;
 }
